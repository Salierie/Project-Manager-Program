using System;
using System.Windows.Forms;

namespace Project_Manager_Pro.GUI
{
    public partial class EditProject : Form
    {
        private string? projectName;
        private string? newProjectName;
        private DateTime currentDate;
        private DateTime newCurrentDate;
        private Button saveBtn;
        private TextBox newProjectNameTextBox;
        private DateTimePicker currentDatePicker;
        private Label projectNameLabel;
        private Label dateLabel;
        private readonly ProjectManagement projectManager;

        internal EditProject(string? projectName, DateTime currentDate, ProjectManagement projectManager)
        {
            this.projectName = projectName;
            this.currentDate = currentDate;
            this.newCurrentDate = currentDate;
            this.projectManager = projectManager;
            InitializeRenameProject();
        }

        private void InitializeRenameProject()
        {
            // Form settings
            this.Text = "Edit Project";
            this.Size = new System.Drawing.Size(400, 200);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Project Name Label
            projectNameLabel = new Label
            {
                Text = "Enter new project name:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };

            // Project Name TextBox
            newProjectNameTextBox = new TextBox
            {
                Location = new System.Drawing.Point(170, 20),
                Size = new System.Drawing.Size(200, 20),
                Text = projectName
            };

            // Date Label
            dateLabel = new Label
            {
                Text = "Project current date:",
                Location = new System.Drawing.Point(20, 60),
                AutoSize = true
            };

            // Date Picker
            currentDatePicker = new DateTimePicker
            {
                Location = new System.Drawing.Point(170, 60),
                Size = new System.Drawing.Size(200, 20),
                Format = DateTimePickerFormat.Short,
                Value = currentDate
            };

            // Save button - moved down
            saveBtn = new Button
            {
                Text = "Save",
                Location = new System.Drawing.Point(150, 100),
                Size = new System.Drawing.Size(75, 23),
                DialogResult = DialogResult.OK
            };
            saveBtn.Click += SaveBtn_Click;

            // Add controls to form
            this.Controls.Add(projectNameLabel);
            this.Controls.Add(newProjectNameTextBox);
            this.Controls.Add(dateLabel);
            this.Controls.Add(currentDatePicker);
            this.Controls.Add(saveBtn);
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            newProjectName = newProjectNameTextBox.Text.Trim();
            newCurrentDate = currentDatePicker.Value;

            if (string.IsNullOrEmpty(newProjectName))
            {
                MessageBox.Show("Project name cannot be empty.", "Invalid Name",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Add validation for reserved project names (case-insensitive)
            if (newProjectName.Equals("Start", StringComparison.OrdinalIgnoreCase) || 
                newProjectName.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("'Start' and 'End' are reserved names and cannot be used as project names.", 
                    "Invalid Project Name", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newProjectName == projectName && newCurrentDate == currentDate)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }

            // Update project name if changed
            if (newProjectName != projectName)
            {
                projectManager.ProjectTree.ChangeTaskName(projectName, newProjectName);
            }

            // Update current date if changed
            if (newCurrentDate != currentDate)
            {
                projectManager.SetCurrenDateOfProject(newCurrentDate);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string? GetNewProjectName()
        {
            return newProjectName;
        }

        public DateTime GetNewCurrentDate()
        {
            return newCurrentDate;
        }
    }
}

