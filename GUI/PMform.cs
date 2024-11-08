using System;
using System.Windows.Forms;
using System.Text.Json;
using System.Collections.Generic;

namespace Project_Manager_Pro.GUI
{
    public partial class PMform : Form
    {
        public string? projectName;
        public DateTime currentDate;
        public string? taskName;

        private DataGridView taskViewTable;
        private Button editBtn, editTRsBtn, editProjectBtn, addTaskBtn, resourceManagementBtn, viewTaskInfoBtn;
        private Button ganttChartBtn;
        private ProjectManagement projectManager;

        internal PMform(string? projectName, DateTime currentDate, ProjectManagement? existingProject = null)
        {
            this.projectName = projectName;
            this.currentDate = currentDate;
            this.projectManager = existingProject ?? new ProjectManagement(projectName ?? "Untitled Project");
            this.projectManager.SetCurrenDateOfProject(currentDate);
            InitializeComponent();


            AddSampleData(); // Add data sample data 
            RefreshTaskView();
        }

        public class DataGridViewWordWrapper : IPrintWordWithEmptySpace
        {
            private readonly DataGridViewRow row;
            private readonly int defaultRowHeight;

            public DataGridViewWordWrapper(DataGridViewRow row, int defaultRowHeight)
            {
                this.row = row;
                this.defaultRowHeight = defaultRowHeight;
            }

            public void PrintWordWithEmptySpace(string word, int MaximumEmptySpace)
            {
                // Calculate how many lines this text will take
                int textWidth = TextRenderer.MeasureText(word, row.DataGridView.Font).Width;
                int cellWidth = MaximumEmptySpace;
                int numberOfLines = (int)Math.Ceiling((double)textWidth / cellWidth);
                
                // Set row height based on number of lines (with padding)
                int newHeight = Math.Max(defaultRowHeight, numberOfLines * defaultRowHeight);
                if (row.Height < newHeight)
                {
                    row.Height = newHeight;
                }
            }
        }

        private void InitializeComponent(){

            this.Text = $"Project Manager - {projectName}";
            this.Size = new System.Drawing.Size(1500, 500);

            taskViewTable = new DataGridView{
                Location = new System.Drawing.Point(10, 20),
                Size = new System.Drawing.Size(1060, 400),
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            taskViewTable.Columns.Add("TaskID", "Task ID");
            taskViewTable.Columns.Add("TaskName", "Task Name");
            taskViewTable.Columns.Add("Duration", "Duration");
            taskViewTable.Columns.Add("StartDate", "Start Date");
            taskViewTable.Columns.Add("FinishDate", "Finish Date");
            taskViewTable.Columns.Add("Status", "Status");
            taskViewTable.Columns.Add("Priority", "Priority");
            taskViewTable.Columns.Add("PercentageComplete", "Percentage complete");
            taskViewTable.Columns.Add("WorkingHours/day", "Working hours/day");

            // Add the Description column
            DataGridViewButtonColumn descriptionColumn = new DataGridViewButtonColumn{
                Name = "Description",
                HeaderText = "Description",
                Text = "View",
                UseColumnTextForButtonValue = true
            };
            taskViewTable.Columns.Add(descriptionColumn);

            // Add click handler for the DataGridView
            taskViewTable.CellClick += TaskViewTable_CellClick;

            //Rename Project Button
            editProjectBtn = new Button{
                Text = "Edit Project Properties",
                Location = new System.Drawing.Point(1100, 25),
                Size = new System.Drawing.Size(140, 30)
            };
            editProjectBtn.Click += new EventHandler(EditProjectBtn_Click);

            //Adding task button
            addTaskBtn = new Button{
                Text = "Adding New Task",
                Location = new System.Drawing.Point(1100, 80),
                Size = new System.Drawing.Size(140, 30)
            };
            addTaskBtn.Click += new EventHandler(AddTaskBtn_Click);

            //Edit Task button
            editBtn = new Button{
                Text = "Edit Task Information",
                Location = new System.Drawing.Point(1250, 80),
                Size = new System.Drawing.Size(140, 30)
            };
            editBtn.Click += new EventHandler(EditBtn_Click);


            //Edit Resource button
            editTRsBtn = new Button{
                Text = "Edit Task Resource",
                Location = new System.Drawing.Point(1100, 135),
                Size = new System.Drawing.Size(140, 30)
            };
            editTRsBtn.Click += new EventHandler(EditTRsBtn_Click);

            //Resource Manager Button
            resourceManagementBtn = new Button{
                Text = "Resource Manager",
                Location = new System.Drawing.Point(1100, 190),
                Size = new System.Drawing.Size(140, 30)
            };
            resourceManagementBtn.Click += new EventHandler(ResourceManagementBtn_Click);

            //Gantt Chart Button
            ganttChartBtn = new Button{
                Text = "Gantt Chart",
                Location = new System.Drawing.Point(1100, 245),
                Size = new System.Drawing.Size(140, 30)
            };
            ganttChartBtn.Click += new EventHandler(GanttChartBtn_Click);

            //View Task Resource Info Button
            Button viewTaskResourceInfoBtn = new Button
            {
                Text = "View Task Resources",
                Location = new System.Drawing.Point(1250, 135),
                Size = new System.Drawing.Size(140, 30)
            };
            viewTaskResourceInfoBtn.Click += ViewTaskResourceInfoBtn_Click;

            // Add to control
            this.Controls.Add(taskViewTable);
            this.Controls.Add(editProjectBtn);
            this.Controls.Add(addTaskBtn);
            this.Controls.Add(editBtn);
            this.Controls.Add(editTRsBtn);
            this.Controls.Add(resourceManagementBtn);
            this.Controls.Add(ganttChartBtn);
            this.Controls.Add(viewTaskInfoBtn);
            this.Controls.Add(viewTaskResourceInfoBtn);

            taskViewTable.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            taskViewTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            taskViewTable.CellPainting += TaskViewTable_CellPainting;
        }

        private void AddTaskBtn_Click(object sender, EventArgs e)
        {
            AddingTaskForm addTaskForm = new AddingTaskForm(projectManager);
            if (addTaskForm.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine("Task added, refreshing view...");
                Console.WriteLine($"Current task count: {projectManager.ProjectTree.TaskNameandIDDic.Count}");
                RefreshTaskView();
                Console.WriteLine($"DataGridView row count: {taskViewTable.Rows.Count}");
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (taskViewTable.SelectedRows.Count > 0)
            {
                string selectedTaskName = taskViewTable.SelectedRows[0].Cells["TaskName"].Value?.ToString() ?? string.Empty;
                EditTaskForm editForm = new EditTaskForm(selectedTaskName, projectManager);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshTaskView();
                }
            }
            else
            {
                MessageBox.Show("Please select a task to edit.", "No Task Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void EditTRsBtn_Click(object sender, EventArgs e){
            if (taskViewTable.SelectedRows.Count > 0)
            {
                string selectedTaskName = taskViewTable.SelectedRows[0].Cells["TaskName"].Value?.ToString() ?? string.Empty;
                EditTaskRSForm editRSForm = new EditTaskRSForm(selectedTaskName, projectManager);
                if (editRSForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshTaskView();
                }
            }
            else
            {
                MessageBox.Show("Please select a task to edit resources.", "No Task Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ResourceManagementBtn_Click(object sender, EventArgs e){
            RSform resourceForm = new RSform(projectName, projectManager);
            resourceForm.ShowDialog();
        }
        
        private void GanttChartBtn_Click(object sender, EventArgs e){
            projectManager.CreateOrUpdateGanttChart();
            if (projectManager.ProjectGanttChart != null)
            {
                GCform ganttForm = new GCform(projectName ?? "Untitled Project", projectManager);
                ganttForm.ShowDialog();
            }
        }

        private void RefreshTaskView()
        {
            taskViewTable.SuspendLayout();
            taskViewTable.Rows.Clear();

            var tasks = projectManager.GetTasks();
            foreach (var task in tasks)
            {
                if (task.TaskNodeLevelInTree == 0) continue;

                int rowIndex = taskViewTable.Rows.Add(
                    task.TaskID,
                    task.TaskName,
                    task.Duration.ToString(),
                    task.StartDate != DateTime.MinValue ? task.StartDate.ToString("MM/dd/yyyy") : "",
                    task.EndDate != DateTime.MinValue ? task.EndDate.ToString("MM/dd/yyyy") : "",
                    task.Status,
                    task.Priority,
                    task.PercentageCompleted.ToString() + "%",
                    task.TaskWorkingHoursPerDay.ToString()
                );

                var row = taskViewTable.Rows[rowIndex];
                var wrapper = new DataGridViewWordWrapper(row, taskViewTable.RowTemplate.Height);
                
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        wrapper.PrintWordWithEmptySpace(
                            cell.Value.ToString() ?? string.Empty,
                            cell.OwningColumn.Width
                        );
                    }
                }
            }

            taskViewTable.ResumeLayout();
        }

        private void EditProjectBtn_Click(object sender, EventArgs e)
        {
            EditProject editProjectForm = new EditProject(projectName, currentDate, projectManager);
            if (editProjectForm.ShowDialog() == DialogResult.OK)
            {
                string? newProjectName = editProjectForm.GetNewProjectName();
                DateTime newCurrentDate = editProjectForm.GetNewCurrentDate();
                
                if (newProjectName != null)
                {
                    projectName = newProjectName;
                    this.Text = $"Project Manager - {projectName}";
                }
                
                if (newCurrentDate != currentDate)
                {
                    currentDate = newCurrentDate;
                }
                
                RefreshTaskView();
            }
        }

        private void AddSampleData()
        {
            TestCase.AddSampleTasks(projectManager);
            TestCase.AddSampleResources(projectManager);
            TestCase.AssignResourcesToTasks(projectManager);
        }


        private void ViewTaskResourceInfoBtn_Click(object sender, EventArgs e)
        {
            if (taskViewTable.SelectedRows.Count > 0)
            {
                string selectedTaskName = taskViewTable.SelectedRows[0].Cells["TaskName"].Value?.ToString() ?? string.Empty;
                using (var form = new TaskResourceInfo(selectedTaskName, projectManager))
                {
                    form.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Please select a task first.", "No Task Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TaskViewTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == taskViewTable.Columns["Description"].Index && e.RowIndex >= 0)
            {
                string taskName = taskViewTable.Rows[e.RowIndex].Cells["TaskName"].Value?.ToString() ?? string.Empty;
                ShowTaskDescription(taskName);
            }
        }

        private void ShowTaskDescription(string taskName)
        {
            var task = projectManager.ProjectTree.FindTaskNode(taskName);
            if (task == null || task.Desription.Count == 0)
            {
                MessageBox.Show("No description available for this task.", 
                    "Task Description", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                return;
            }

            string descriptionText = string.Join(Environment.NewLine, 
                task.Desription.Select((desc, index) => $"{desc}"));

            MessageBox.Show(descriptionText, 
                $"Description for {taskName}", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Information);
        }

        private void TaskViewTable_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var row = taskViewTable.Rows[e.RowIndex];
                var wrapper = new DataGridViewWordWrapper(row, taskViewTable.RowTemplate.Height);

                // Process each cell in the row
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        wrapper.PrintWordWithEmptySpace(
                            cell.Value.ToString() ?? string.Empty,
                            cell.OwningColumn.Width
                        );
                    }
                }
            }
        }

        private void InitializeTaskViewTable()
        {

            taskViewTable.ColumnWidthChanged += (sender, e) =>
            {
                foreach (DataGridViewRow row in taskViewTable.Rows)
                {
                    var wrapper = new DataGridViewWordWrapper(row, taskViewTable.RowTemplate.Height);
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Value != null)
                        {
                            wrapper.PrintWordWithEmptySpace(
                                cell.Value.ToString() ?? string.Empty,
                                cell.OwningColumn.Width
                            );
                        }
                    }
                }
            };
        }

    }
}
