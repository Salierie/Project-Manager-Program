using System;
using System.Windows.Forms;

namespace Project_Manager_Pro.GUI;

internal partial class GCform : Form
{
    private ProjectManagement projectManager;
    private Panel ganttChartPanel;
    private const int CELL_HEIGHT = 35;
    private const int TASK_INFO_WIDTH = 840;
    private const int DAY_WIDTH = 35;
    private const int HEADER_HEIGHT = 60;
    private const int MARGIN = 20;
    private const int INDENT_WIDTH = 20;
    private const int TIMELINE_MARGIN = 2;
    
    // Updated colors for modern look
    private readonly Color gridLineColor = Color.FromArgb(240, 240, 240);
    private readonly Color criticalTaskColor = Color.FromArgb(255, 76, 76);
    private readonly Color normalTaskColor = Color.FromArgb(0, 120, 212);
    private readonly Color completedColor = Color.FromArgb(67, 160, 71);
    private readonly Color headerBackColor = Color.FromArgb(250, 250, 250);
    private readonly Color alternateRowColor = Color.FromArgb(252, 252, 252);

    // Define columns as a class-level field
    private readonly (string Header, int Width)[] columns = new[]
    {
        ("Task ID", 60),
        ("Task Name", 300),
        ("Duration", 80),
        ("Start Date", 100),
        ("End Date", 100),
        ("Status", 80),
        ("Priority", 80)
    };

    private const int MAX_TIMELINE_DAYS = 30; // Increased to show more days
    private const int TIMELINE_SHIFT = 5; // Days to shift when using timeline controls
    private DateTime timelineViewStart;
    private DateTime timelineViewEnd;

    private TreeOfTasks Tree;

    public GCform(string projectName, ProjectManagement projectManager)
    {
        this.projectManager = projectManager;
        this.Tree = projectManager.ProjectTree;
        
        // Initialize timeline view to first two weeks of project
        var tasks = projectManager.ProjectGanttChart?.TaskBars;
        if (tasks != null && tasks.Any())
        {
            timelineViewStart = tasks.Values.Min(t => t.StartDate);
            timelineViewEnd = timelineViewStart.AddDays(MAX_TIMELINE_DAYS - 1);
        }
        
        InitializeGanttChart(projectName);
        this.WindowState = FormWindowState.Maximized;
    }

    private void InitializeGanttChart(string projectName)
    {
        this.Text = $"Gantt Chart: {projectName}";
        this.WindowState = FormWindowState.Maximized;
        this.FormBorderStyle = FormBorderStyle.Sizable;
        this.StartPosition = FormStartPosition.CenterParent;

        // Replace week controls with timeline controls
        var controlPanel = new Panel
        {
            Location = new Point(MARGIN, MARGIN),
            Height = 30,
            Dock = DockStyle.Top
        };

        var prevButton = new Button
        {
            Text = "◄ Move Timeline",
            Size = new Size(120, 25),
            Location = new Point(TASK_INFO_WIDTH + TIMELINE_MARGIN, 0)
        };
        prevButton.Click += (s, e) => ShiftTimelineView(-TIMELINE_SHIFT);

        var nextButton = new Button
        {
            Text = "Move Timeline ►",
            Size = new Size(120, 25),
            Location = new Point(TASK_INFO_WIDTH + TIMELINE_MARGIN + 130, 0)
        };
        nextButton.Click += (s, e) => ShiftTimelineView(TIMELINE_SHIFT);

        controlPanel.Controls.Add(prevButton);
        controlPanel.Controls.Add(nextButton);
        this.Controls.Add(controlPanel);

        // Initialize Panel with Double Buffering
        ganttChartPanel = new Panel
        {
            Location = new Point(MARGIN, controlPanel.Bottom + 10),
            BorderStyle = BorderStyle.FixedSingle,
            AutoScroll = true,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 0, 0, MARGIN)
        };
        typeof(Panel).InvokeMember("DoubleBuffered",
            System.Reflection.BindingFlags.SetProperty |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic,
            null, ganttChartPanel, new object[] { true });

        this.Controls.Add(ganttChartPanel);
        ganttChartPanel.Paint += GanttChartPanel_Paint;
        this.Resize += GCform_Resize;
    }

    private void GCform_Resize(object sender, EventArgs e)
    {
        // Adjust panel size to leave margin
        ganttChartPanel.Size = new Size(
            this.ClientSize.Width - 40,
            this.ClientSize.Height - 40
        );
        
        // Ensure minimum width for readability
        if (ganttChartPanel.Width < TASK_INFO_WIDTH + 400)
        {
            this.Width = TASK_INFO_WIDTH + 460; // Add margin
        }
        
        ganttChartPanel.Invalidate();
    }

    private void GanttChartPanel_Paint(object sender, PaintEventArgs e)
    {
        if (projectManager.ProjectGanttChart == null) return;

        Graphics g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        // Calculate total width needed
        var tasks = projectManager.ProjectGanttChart.TaskBars;
        int totalDays = (int)(timelineViewEnd - timelineViewStart).TotalDays + 1;
        int timelineWidth = totalDays * DAY_WIDTH;
        int totalWidth = TASK_INFO_WIDTH + TIMELINE_MARGIN + timelineWidth;
        
        // Set panel's virtual size
        ganttChartPanel.AutoScrollMinSize = new Size(
            Math.Max(totalWidth, ganttChartPanel.Width),
            Math.Max((tasks.Count + 1) * CELL_HEIGHT + HEADER_HEIGHT, ganttChartPanel.Height)
        );

        // Create clipping region for timeline
        Rectangle timelineClip = new Rectangle(
            TASK_INFO_WIDTH + TIMELINE_MARGIN,
            0,
            ganttChartPanel.Width - TASK_INFO_WIDTH - TIMELINE_MARGIN,
            ganttChartPanel.Height
        );

        // Save the original clip region
        Region originalClip = g.Clip;

        // Draw fixed headers (don't scroll horizontally)
        using (Font headerFont = new Font("Segoe UI", 9, FontStyle.Regular))
        {
            DrawHeaders(g, headerFont, columns);
        }

        // Set clip region for timeline and draw timeline
        g.SetClip(timelineClip);
        g.TranslateTransform(-ganttChartPanel.AutoScrollPosition.X, -ganttChartPanel.AutoScrollPosition.Y);
        DrawTimeline(g);
        
        // Draw tasks with the same clipping
        DrawTasksGrid(g);

        // Restore original clip region
        g.Clip = originalClip;
    }

    private void DrawProjectHeader(Graphics g)
    {
        using (Font headerFont = new Font("Arial", 16, FontStyle.Bold))
        using (Font normalFont = new Font("Arial", 10))
        {
            g.DrawString($"Project: {projectManager.ProjectTree.RootTask.TaskName}", 
                headerFont, Brushes.Black, 20, 20);
            g.DrawString($"Current Date: {projectManager.ProjectTree.CurrentDate:MM/dd/yyyy}", 
                normalFont, Brushes.Black, 20, 60);
        }
    }

    private void DrawTimeline(Graphics g)
    {
        int timelineWidth = (int)(timelineViewEnd - timelineViewStart).TotalDays * DAY_WIDTH + DAY_WIDTH;

        // Use a larger, more prominent font for dates
        using (Font timelineFont = new Font("Segoe UI", 9, FontStyle.Regular))
        using (var headerBrush = new SolidBrush(headerBackColor))
        using (var borderPen = new Pen(Color.FromArgb(200, 200, 200)))
        {
            int x = TASK_INFO_WIDTH + TIMELINE_MARGIN;
            int dayStep = 5; // Show dates every 5 days
            
            // Draw header background
            g.FillRectangle(headerBrush, x, 0, timelineWidth, HEADER_HEIGHT);
            
            // Draw vertical separator line between info table and timeline
            using (var separatorPen = new Pen(Color.FromArgb(180, 180, 180), 2))
            {
                g.DrawLine(separatorPen, x - TIMELINE_MARGIN, 0, x - TIMELINE_MARGIN, ganttChartPanel.Height);
            }
            
            // Draw dates
            for (DateTime date = timelineViewStart; date <= timelineViewEnd; date = date.AddDays(dayStep))
            {
                // Create date column border
                Rectangle dateRect = new Rectangle(x, HEADER_HEIGHT - 30, DAY_WIDTH * dayStep, 30);
                g.DrawRectangle(borderPen, dateRect);
                
                // Draw date with better formatting
                string dateStr = date.ToString("MM/dd");
                SizeF textSize = g.MeasureString(dateStr, timelineFont);
                float textX = x + (DAY_WIDTH * dayStep - textSize.Width) / 2;
                g.DrawString(dateStr, timelineFont, Brushes.Black, textX, HEADER_HEIGHT - 25);
                
                // Draw vertical grid line
                using (Pen gridPen = new Pen(gridLineColor))
                {
                    g.DrawLine(gridPen, x, HEADER_HEIGHT, x, ganttChartPanel.Height);
                }
                
                x += DAY_WIDTH * dayStep;
            }
            
            // Draw final vertical line
            using (Pen gridPen = new Pen(gridLineColor))
            {
                g.DrawLine(gridPen, x, HEADER_HEIGHT, x, ganttChartPanel.Height);
            }
        }
    }

    private void DrawTasksGrid(Graphics g)
    {
        if (projectManager.ProjectGanttChart?.TasksWithOrder == null) return;

        var tasks = projectManager.ProjectGanttChart.TaskBars;
        int y = HEADER_HEIGHT;

        using (Font headerFont = new Font("Segoe UI", 9, FontStyle.Regular))
        using (Font taskFont = new Font("Segoe UI", 9))
        {
            // Draw task information (not clipped)
            using (var originalClip = g.Clip)
            {
                g.SetClip(new Rectangle(0, 0, TASK_INFO_WIDTH, ganttChartPanel.Height));
                DrawHeaders(g, headerFont, columns);
                y += CELL_HEIGHT;

                foreach (string taskName in projectManager.ProjectGanttChart.TasksWithOrder)
                {
                    var task = tasks[taskName];
                    DrawTaskInfoPart(g, task, y, taskFont, columns);
                    y += CELL_HEIGHT;
                }
                g.Clip = originalClip;
            }

            // Draw Gantt bars (clipped to timeline area)
            y = HEADER_HEIGHT + CELL_HEIGHT;
            using (var timelineClip = new Region(new Rectangle(
                TASK_INFO_WIDTH + TIMELINE_MARGIN,
                0,
                ganttChartPanel.Width - TASK_INFO_WIDTH - TIMELINE_MARGIN,
                ganttChartPanel.Height)))
            {
                g.Clip = timelineClip;
                foreach (string taskName in projectManager.ProjectGanttChart.TasksWithOrder)
                {
                    var task = tasks[taskName];
                    if (task.StartDate <= timelineViewEnd && task.FinishDate >= timelineViewStart)
                    {
                        DrawTaskGanttPart(g, task, y);
                    }
                    y += CELL_HEIGHT;
                }
            }
        }
    }

    // New method to draw just the task information part
    private void DrawTaskInfoPart(Graphics g, GanttChartBar task, int y, Font font, (string Header, int Width)[] columns)
    {
        // Draw row background
        using (var rowBrush = new SolidBrush(y % (CELL_HEIGHT * 2) == 0 ? alternateRowColor : Color.White))
        {
            g.FillRectangle(rowBrush, 0, y, TASK_INFO_WIDTH, CELL_HEIGHT);
        }

        // Draw task information columns
        using (var format = new StringFormat { 
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        })
        {
            int x = 0;
            foreach (var (Header, Width) in columns)
            {
                Rectangle rect = new Rectangle(x, y, Width, CELL_HEIGHT);
                string value = GetColumnValue(task, Header);

                // Add indentation for task name based on BarLevel
                if (Header == "Task Name" && task.BarLevel > 1)
                {
                    int indentLevel = task.BarLevel - 1;
                    int indentWidth = indentLevel * INDENT_WIDTH;
                    rect.X += indentWidth;
                    rect.Width -= indentWidth;
                    
                    // Ensure minimum width for text
                    if (rect.Width < 50)
                    {
                        rect.Width = 50;
                    }
                }

                g.DrawString(value, font, Brushes.Black, rect, format);
                x += Width;
            }
        }
    }

    // New method to draw just the Gantt bar part
    private void DrawTaskGanttPart(Graphics g, GanttChartBar task, int y)
    {
        int barX = TASK_INFO_WIDTH + TIMELINE_MARGIN + (int)((task.StartDate - timelineViewStart).TotalDays * DAY_WIDTH);
        int barWidth = Math.Max(DAY_WIDTH, (int)((task.FinishDate - task.StartDate).TotalDays + 1) * DAY_WIDTH);

        // Draw bar with rounded corners
        using (var path = CreateRoundedRectangle(barX, y + 8, barWidth, CELL_HEIGHT - 16, 4))
        using (var brush = new SolidBrush(task.Critical ? criticalTaskColor : normalTaskColor))
        {
            g.FillPath(brush, path);

            // Draw completion percentage
            if (task.PercentageCompleted > 0)
            {
                int completedWidth = (int)(barWidth * (task.PercentageCompleted / 100.0));
                using (var completedPath = CreateRoundedRectangle(barX, y + 8, completedWidth, CELL_HEIGHT - 16, 4))
                using (var completedBrush = new SolidBrush(completedColor))
                {
                    g.FillPath(completedBrush, completedPath);
                }
            }

            // Draw percentage text below the bar
            using (var font = new Font("Segoe UI", 8))
            {
                string percentText = $"{task.PercentageCompleted}%";
                SizeF textSize = g.MeasureString(percentText, font);
                float textX = barX + (barWidth - textSize.Width) / 2;
                float textY = y + CELL_HEIGHT - 14; // Position below the bar
                
                // Draw white background for better readability
                using (var bgBrush = new SolidBrush(Color.FromArgb(250, 250, 250)))
                {
                    g.FillRectangle(bgBrush, textX - 2, textY, textSize.Width + 4, textSize.Height);
                }
                
                g.DrawString(percentText, font, Brushes.Black, textX, textY);
            }
        }

        // Draw dependency arrows after the bar
        DrawDependencyArrows(g, task, y);
    }

    private void DrawDependencyArrows(Graphics g, GanttChartBar task, int y)
    {
        if (task.Depending_vertices == null) return;

        using (var arrowPen = new Pen(Color.FromArgb(120, 120, 120), 2))
        {
            arrowPen.CustomEndCap = new System.Drawing.Drawing2D.AdjustableArrowCap(6, 6);

            foreach (var dependency in task.Depending_vertices)
            {
                var dependingTask = projectManager.ProjectGanttChart.TaskBars[dependency.Key];
                if (dependingTask == null) continue;

                // Calculate start and end points based on dependency type
                Point startPoint = new Point();
                Point endPoint = new Point();

                int taskY = y + CELL_HEIGHT / 2;
                int dependingTaskY = HEADER_HEIGHT + CELL_HEIGHT + 
                    (projectManager.ProjectGanttChart.TasksWithOrder.IndexOf(dependency.Key) * CELL_HEIGHT) + 
                    CELL_HEIGHT / 2;

                switch (dependency.Value.Type)
                {
                    case "FS":
                        startPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((dependingTask.FinishDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            dependingTaskY);
                        endPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((task.StartDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            taskY);
                        break;

                    case "SS": 
                        startPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((dependingTask.StartDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            dependingTaskY);
                        endPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((task.StartDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            taskY);
                        break;

                    case "FF": 
                        startPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((dependingTask.FinishDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            dependingTaskY);
                        endPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((task.FinishDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            taskY);
                        break;

                    case "SF": 
                        startPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((dependingTask.StartDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            dependingTaskY);
                        endPoint = new Point(
                            TASK_INFO_WIDTH + TIMELINE_MARGIN + 
                            (int)((task.FinishDate - timelineViewStart).TotalDays * DAY_WIDTH),
                            taskY);
                        break;
                }

                // Draw arrow with intermediate points for better routing
                List<Point> points = new List<Point> { startPoint };
                
                // Add intermediate points for better arrow routing
                if (Math.Abs(endPoint.Y - startPoint.Y) > CELL_HEIGHT)
                {
                    int midX = (startPoint.X + endPoint.X) / 2;
                    points.Add(new Point(midX, startPoint.Y));
                    points.Add(new Point(midX, endPoint.Y));
                }
                
                points.Add(endPoint);

                // Draw the arrow path
                g.DrawLines(arrowPen, points.ToArray());
            }
        }
    }

    // Helper method to get column value
    private string GetColumnValue(GanttChartBar task, string header)
    {
        return header switch
        {
            "Task ID" => FormatTaskId(Tree.GetTaskID(task.TaskName)),
            "Task Name" => task.TaskName,
            "Duration" => $"{task.Duration} days",
            "Start Date" => task.StartDate.ToString("MM/dd/yyyy"),
            "End Date" => task.FinishDate.ToString("MM/dd/yyyy"),
            "Status" => task.Status,
            "Priority" => task.Priority.ToString(),
            _ => ""
        };
    }

    // Add new helper method to format TaskID
    private string FormatTaskId(string taskId)
    {
        return string.Join(".", taskId.ToCharArray());
    }

    private System.Drawing.Drawing2D.GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        var rect = new Rectangle(x, y, width, height);
        int diameter = radius * 2;

        // Create rounded rectangle
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private void DrawHeaders(Graphics g, Font headerFont, (string Header, int Width)[] columns)
    {
        int x = 10;
        using (var headerBrush = new SolidBrush(headerBackColor))
        using (var format = new StringFormat { LineAlignment = StringAlignment.Center })
        {
            g.FillRectangle(headerBrush, 0, HEADER_HEIGHT, TASK_INFO_WIDTH, CELL_HEIGHT);
            
            foreach (var column in columns)
            {
                Rectangle headerRect = new Rectangle(x, HEADER_HEIGHT, column.Width, CELL_HEIGHT);
                g.DrawString(column.Header, headerFont, Brushes.Black, headerRect, format);
                x += column.Width;
            }
        }
    }

    // Add timeline navigation controls
    private void AddTimelineControls()
    {
        var prevButton = new Button
        {
            Text = "◄",
            Size = new Size(30, 25),
            Location = new Point(10, 5)
        };
        prevButton.Click += (s, e) => ShiftTimelineView(-7);

        var nextButton = new Button
        {
            Text = "►",
            Size = new Size(30, 25),
            Location = new Point(45, 5)
        };
        nextButton.Click += (s, e) => ShiftTimelineView(7);

        this.Controls.Add(prevButton);
        this.Controls.Add(nextButton);
    }

    private void ShiftTimelineView(int days)
    {
        var tasks = projectManager.ProjectGanttChart?.TaskBars;
        if (tasks == null || !tasks.Any()) return;

        DateTime minDate = tasks.Values.Min(t => t.StartDate);
        DateTime maxDate = tasks.Values.Max(t => t.FinishDate);

        DateTime newStart = timelineViewStart.AddDays(days);
        DateTime newEnd = timelineViewEnd.AddDays(days);

        if (newStart >= minDate && newEnd <= maxDate)
        {
            timelineViewStart = newStart;
            timelineViewEnd = newEnd;
            ganttChartPanel.Invalidate();
        }
    }
}