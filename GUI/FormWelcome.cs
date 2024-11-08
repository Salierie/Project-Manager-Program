using System;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;

namespace Project_Manager_Pro.GUI
{
    public partial class FormWelcome : Form
    {
        public string projectName;
        private TextBox projectNameTextBox;
        private DateTimePicker currentDatePicker;
        private Button openButton;

        public FormWelcome()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Welcome!";
            this.Location = new System.Drawing.Point(100, 100);
            this.Size = new System.Drawing.Size(400, 200);

            Label welcomeLabel = new Label
            {
                Text = "WELCOME USER!",
                Location = new System.Drawing.Point(120, 20),
                AutoSize = true
            };
            
            Label projectLabel = new Label
            {
                Text = "Enter project name:",
                Location = new System.Drawing.Point(30, 60),
                AutoSize = true
            };

            projectNameTextBox = new TextBox
            {
                Location = new System.Drawing.Point(160, 60),
                Width = 180
            };

            Label dateLabel = new Label
            {
                Text = "Select project date:",
                Location = new System.Drawing.Point(30, 90),
                AutoSize = true
            };

            currentDatePicker = new DateTimePicker
            {
                Location = new System.Drawing.Point(160, 90),
                Width = 180,
                Format = DateTimePickerFormat.Short
            };

            openButton = new Button
            {
                Text = "Open",
                Location = new System.Drawing.Point(150, 130),
                AutoSize = true
            };
            openButton.Click += new EventHandler(OpenButton_Click);

            this.Controls.Add(welcomeLabel);
            this.Controls.Add(projectLabel);
            this.Controls.Add(projectNameTextBox);
            this.Controls.Add(dateLabel);
            this.Controls.Add(currentDatePicker);
            this.Controls.Add(openButton);
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            projectName = projectNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(projectName))
            {
                MessageBox.Show("Please enter project name!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (projectName.Equals("Start", StringComparison.OrdinalIgnoreCase) || 
                projectName.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("'Start' and 'End' are reserved names and cannot be used as project names.", 
                    "Invalid Project Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string filePath = Path.Combine(Application.StartupPath, $"{projectName}.xml");
            ProjectManagement? existingProject = null;

            if (File.Exists(filePath))
            {
                try
                {
                    var serializer = new DataContractSerializer(typeof(ProjectData));
                    using (FileStream fs = new FileStream(filePath, FileMode.Open))
                    {
                        var projectData = (ProjectData)serializer.ReadObject(fs);
                        existingProject = new ProjectManagement(projectData.ProjectName);
                        existingProject.SetCurrenDateOfProject(projectData.CurrentDate);

                        // Load resources first
                        foreach (var resource in projectData.Resources)
                        {
                            if (resource.Type == "Work")
                            {
                                existingProject.AddWorkResource(resource.ResourceName);
                                existingProject.SetStandardRateOfWorkResource(resource.ResourceName, resource.StandardRate);
                                existingProject.SetOvertimeRateOfWorkResource(resource.ResourceName, resource.OvertimeRate);
                                existingProject.SetAvailableCapacityOfWorkResource(resource.ResourceName, resource.MaxCapacity);
                                existingProject.SetAccrueTypeOfWorkResource(resource.ResourceName, resource.Accrue);
                            }
                            else if (resource.Type == "Material")
                            {
                                existingProject.AddMaterialResource(resource.ResourceName);
                                existingProject.SetStandardRateOfMaterialResource(resource.ResourceName, resource.StandardRate);
                            }
                        }

                        // Group tasks by their hierarchy level
                        var parentTasks = projectData.Tasks
                            .Where(t => t.TaskID.Length == 1)
                            .OrderBy(t => int.Parse(t.TaskID));

                        var childTasks = projectData.Tasks
                            .Where(t => t.TaskID.Length > 1)
                            .OrderBy(t => t.TaskID);

                        // Load parent tasks first
                        foreach (var task in parentTasks)
                        {
                            try
                            {
                                existingProject.AddTask(task.TaskName);
                                
                                if (task.StartDate != DateTime.MinValue && task.EndDate != DateTime.MinValue)
                                {
                                    existingProject.SetTimelineOfTask(task.TaskName, task.StartDate, task.EndDate);
                                }
                                
                                if (!string.IsNullOrEmpty(task.Status))
                                {
                                    existingProject.UpdateStatusOfTask(task.TaskName, task.Status);
                                }
                                
                                if (!string.IsNullOrEmpty(task.Priority))
                                {
                                    existingProject.SetPriorityOfTask(task.TaskName, task.Priority);
                                }

                                foreach (var resource in task.ResourceAndCapacity)
                                {
                                    if (!string.IsNullOrEmpty(resource.Key))
                                    {
                                        existingProject.AddResourceToTask(resource.Key, task.TaskName);
                                        existingProject.AddCapacityToResourceOfTask(resource.Key, task.TaskName, resource.Value);
                                    }
                                }
                            }
                            catch (Exception taskEx)
                            {
                                Console.WriteLine($"Error loading parent task {task.TaskName}: {taskEx.Message}");
                            }
                        }

                        // Then load child tasks
                        foreach (var task in childTasks)
                        {
                            try
                            {
                                string parentTaskId = task.TaskID.Substring(0, 1);
                                var parentTask = projectData.Tasks.FirstOrDefault(t => t.TaskID == parentTaskId);
                                
                                if (parentTask != null)
                                {
                                    existingProject.AddSubtaskToTask(task.TaskName, parentTask.TaskName);
                                    
                                    if (task.StartDate != DateTime.MinValue && task.EndDate != DateTime.MinValue)
                                    {
                                        existingProject.SetTimelineOfTask(task.TaskName, task.StartDate, task.EndDate);
                                    }
                                    
                                    if (!string.IsNullOrEmpty(task.Status))
                                    {
                                        existingProject.UpdateStatusOfTask(task.TaskName, task.Status);
                                    }
                                    
                                    if (!string.IsNullOrEmpty(task.Priority))
                                    {
                                        existingProject.SetPriorityOfTask(task.TaskName, task.Priority);
                                    }

                                    foreach (var resource in task.ResourceAndCapacity)
                                    {
                                        if (!string.IsNullOrEmpty(resource.Key))
                                        {
                                            existingProject.AddResourceToTask(resource.Key, task.TaskName);
                                            existingProject.AddCapacityToResourceOfTask(resource.Key, task.TaskName, resource.Value);
                                        }
                                    }
                                }
                            }
                            catch (Exception taskEx)
                            {
                                Console.WriteLine($"Error loading child task {task.TaskName}: {taskEx.Message}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading project: {ex.Message}\nStack trace: {ex.StackTrace}", 
                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            try
            {
                PMform mainForm = new PMform(projectName, currentDatePicker.Value, existingProject);
                this.Hide();
                mainForm.FormClosed += (s, args) => this.Close();
                mainForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening project: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
