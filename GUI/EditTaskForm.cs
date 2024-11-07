using System;
using System.Windows.Forms;

namespace Project_Manager_Pro.GUI
{
    public partial class EditTaskForm : Form{

        public string? taskName;
        private TextBox taskNameTextBox, descriptionTextBox, durationTextBox;
        private TextBox timePerDayTextBox, dependingTaskTextBox, lagTimeTextBox;
        private ComboBox statusComboBox, priorityComboBox, dependencyTypeComboBox;
        private Button deleteDependencyBtn, deleteTaskBtn, deleteDescriptionBtn, saveBtn;
        private GroupBox dependencyGroupBox;        
        private ProjectManagement projectManager;
        internal EditTaskForm(string taskName, ProjectManagement projectManager){
            this.taskName = taskName;
            this.projectManager = projectManager;
            InitializeTaskEdit();
        }
        private void InitializeTaskEdit(){
            // Form setup
            this.Text = $"Edit Task";
            this.Size = new Size(800, 600);

            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Left column controls
            Label taskNameLabel = new Label
            {
                Text = "Edit Task Name:",
                Location = new Point(20, 20),
                Size = new Size(100, 20)
            };

            taskNameTextBox = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(300, 20)
            };

            Label statusLabel = new Label
            {
                Text = "Edit Status:",
                Location = new Point(20, 80),
                Size = new Size(100, 20)
            };

            statusComboBox = new ComboBox
            {
                Location = new Point(20, 105),
                Size = new Size(300, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            statusComboBox.Items.AddRange(new string[] {"Not Start", "In Progress", "Complete"});

            Label durationLabel = new Label
            {
                Text = "Edit Duration:",
                Location = new Point(20, 140),
                Size = new Size(100, 20)
            };

            durationTextBox = new TextBox
            {
                Location = new Point(20, 165),
                Size = new Size(300, 20)
            };

            // Right column controls
            Label descriptionLabel = new Label
            {
                Text = "Edit Description:",
                Location = new Point(420, 20),
                Size = new Size(100, 20)
            };

            descriptionTextBox = new TextBox
            {
                Location = new Point(420, 45),
                Size = new Size(300, 20)
            };

            Label priorityLabel = new Label
            {
                Text = "Edit Priority:",
                Location = new Point(420, 80),
                Size = new Size(100, 20)
            };

            priorityComboBox = new ComboBox
            {
                Location = new Point(420, 105),
                Size = new Size(300, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            priorityComboBox.Items.AddRange(new string[] { "Low", "Medium", "High"});

            Label timePerDayLabel = new Label
            {
                Text = "Edit time limit (hrs/day):",
                Location = new Point(420, 140),
                Size = new Size(150, 20)
            };

            timePerDayTextBox = new TextBox
            {
                Location = new Point(420, 165),
                Size = new Size(300, 20)
            };

            // Dependency section
            Label dependencyLabel = new Label
            {
                Text = "Add Dependency:",
                Location = new Point(20, 200),
                Size = new Size(100, 20)
            };

            dependencyTypeComboBox = new ComboBox
            {
                Location = new Point(20, 225),
                Size = new Size(300, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            dependencyTypeComboBox.Items.AddRange(new string[] { "SS", "FF", "FS", "SF" });

            // Dependency GroupBox
            dependencyGroupBox = new GroupBox
            {
                Location = new Point(20, 265),
                Size = new Size(700, 150),
                Text = "Dependency Details"
            };

            Label dependingTaskLabel = new Label
            {
                Text = "Enter Depending task:",
                Location = new Point(10, 30),
                Size = new Size(120, 20)
            };

            dependingTaskTextBox = new TextBox
            {
                Location = new Point(130, 27),
                Size = new Size(300, 20)
            };

            Label lagTimeLabel = new Label
            {
                Text = "Edit Lag time:",
                Location = new Point(10, 70),
                Size = new Size(100, 20)
            };

            lagTimeTextBox = new TextBox
            {
                Location = new Point(130, 67),
                Size = new Size(150, 20)
            };

            deleteDependencyBtn = new Button
            {
                Text = "Delete Dependency",
                Location = new Point(500, 67),
                Size = new Size(150, 30)
            };

            // Bottom buttons
            deleteTaskBtn = new Button
            {
                Text = "Delete Task",
                Location = new Point(20, 450),
                Size = new Size(100, 30)
            };

            deleteDescriptionBtn = new Button
            {
                Text = "Delete Description",
                Location = new Point(420, 450),
                Size = new Size(150, 30)
            };

            saveBtn = new Button
            {
                Text = "Save",
                Location = new Point(350, 500),
                Size = new Size(100, 30)
            };

            // Add controls to dependency GroupBox
            dependencyGroupBox.Controls.AddRange(new Control[]
            {
                dependingTaskLabel,
                dependingTaskTextBox,
                lagTimeLabel,
                lagTimeTextBox,
                deleteDependencyBtn
            });

            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                taskNameLabel,
                taskNameTextBox,
                statusLabel,
                statusComboBox,
                durationLabel,
                durationTextBox,
                descriptionLabel,
                descriptionTextBox,
                priorityLabel,
                priorityComboBox,
                timePerDayLabel,
                timePerDayTextBox,
                dependencyLabel,
                dependencyTypeComboBox,
                dependencyGroupBox,
                deleteTaskBtn,
                deleteDescriptionBtn,
                saveBtn
            });

            // Add event handlers
            saveBtn.Click += SaveBtn_Click;
            deleteTaskBtn.Click += DeleteTaskBtn_Click;
            deleteDescriptionBtn.Click += DeleteDescriptionBtn_Click;
            deleteDependencyBtn.Click += DeleteDependencyBtn_Click;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            string newTaskName = taskNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newTaskName))
            {
                MessageBox.Show("Task name cannot be empty.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check for reserved task names (case-insensitive)
            if (newTaskName.Equals("Start", StringComparison.OrdinalIgnoreCase) || 
                newTaskName.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("'Start' and 'End' are reserved task names and cannot be used.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Save task changes
            if (taskName != newTaskName)
            {
                projectManager.ChangeTaskName(taskName, newTaskName);
            }

            projectManager.UpdateStatusOfTask(taskName, statusComboBox.Text);
            
            if (int.TryParse(durationTextBox.Text, out int duration))
            {
                projectManager.ChangeDurationOfTask(taskName, duration);
            }

            if (!string.IsNullOrWhiteSpace(descriptionTextBox.Text))
            {
                projectManager.AddDescriptionToTask(taskName, descriptionTextBox.Text);
            }

            projectManager.SetPriorityOfTask(taskName, priorityComboBox.Text);

            if (float.TryParse(timePerDayTextBox.Text, out float hours))
            {
                projectManager.SetNumberOfWorkingHoursPerDayOfTask(taskName, hours);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void DeleteTaskBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete this task?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                projectManager.DeleteTask(taskName);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void DeleteDescriptionBtn_Click(object sender, EventArgs e)
        {
            projectManager.DeleteAllDescriptionOfTask(taskName);
            descriptionTextBox.Clear();
        }

        private void DeleteDependencyBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(dependingTaskTextBox.Text))
            {
                projectManager.DeleteDependency(taskName, dependingTaskTextBox.Text);
                dependingTaskTextBox.Clear();
                lagTimeTextBox.Clear();
            }
        }

    }
}