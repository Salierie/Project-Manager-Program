using System;
using System.Windows.Forms;

namespace Project_Manager_Pro.GUI
{
    internal partial class AddingTaskForm : Form
    {
        private ProjectManagement? projectManager;
        private TextBox? taskNameTextBox, parentTaskTextBox, descriptionTextBox, timePerDayTextBox;
        private DateTimePicker? startDatePicker, endDatePicker;
        private ComboBox? statusComboBox, priorityComboBox;
        private Button? saveButton;

        public AddingTaskForm(ProjectManagement? projectManager)
        {
            this.projectManager = projectManager;
            InitializeAddingTask();
        }

        private void InitializeAddingTask()
        {
            this.Text = "Adding Task";
            this.Size = new System.Drawing.Size(400, 400);

            // Initialize controls
            taskNameTextBox = new TextBox
            {
                Location = new Point(140, 20),
                Size = new Size(200, 20)
            };

            parentTaskTextBox = new TextBox
            {
                Location = new Point(140, 50),
                Size = new Size(200, 20)
            };

            descriptionTextBox = new TextBox
            {
                Location = new Point(140, 80),
                Size = new Size(200, 60),
                Multiline = true
            };

            startDatePicker = new DateTimePicker
            {
                Location = new Point(140, 150),
                Size = new Size(200, 20)
            };

            endDatePicker = new DateTimePicker
            {
                Location = new Point(140, 180),
                Size = new Size(200, 20)
            };

            statusComboBox = new ComboBox
            {
                Location = new Point(140, 210),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            statusComboBox.Items.AddRange(new string[] { "Not Start", "In Progress", "Complete" });
            statusComboBox.SelectedIndex = 0;

            priorityComboBox = new ComboBox
            {
                Location = new Point(140, 240),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            priorityComboBox.Items.AddRange(new string[] { "Low", "Medium", "High" });
            priorityComboBox.SelectedIndex = 0;

            timePerDayTextBox = new TextBox
            {
                Location = new Point(140, 270),
                Size = new Size(200, 20)
            };

            saveButton = new Button
            {
                Text = "Save",
                Location = new Point(150, 300),
                Size = new Size(75, 23),
                DialogResult = DialogResult.OK
            };
            saveButton.Click += SaveButton_Click;

            // Add labels
            AddLabel("Task Name:", 20);
            AddLabel("Parent Task:", 50);
            AddLabel("Description:", 80);
            AddLabel("Start Date:", 150);
            AddLabel("End Date:", 180);
            AddLabel("Status:", 210);
            AddLabel("Priority:", 240);
            AddLabel("Working Hours/Day:", 270);

            // Add controls to form
            Controls.AddRange(new Control[] {
                taskNameTextBox, parentTaskTextBox, descriptionTextBox,
                startDatePicker, endDatePicker, statusComboBox, priorityComboBox,
                timePerDayTextBox, saveButton
            });

            startDatePicker.ValueChanged += startDatePicker_ValueChanged;
            endDatePicker.ValueChanged += endDatePicker_ValueChanged;
        }

        private void AddLabel(string text, int y)
        {
            Label label = new Label
            {
                Text = text,
                Location = new Point(20, y),
                AutoSize = true
            };
            Controls.Add(label);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            string taskName = taskNameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(taskName))
            {
                MessageBox.Show("Task name is required!", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (taskName.Equals("Start", StringComparison.OrdinalIgnoreCase) || 
                taskName.Equals("End", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("'Start' and 'End' are reserved task names and cannot be used.", 
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (startDatePicker.Value >= endDatePicker.Value)
            {
                MessageBox.Show("Start date must be earlier than end date!", 
                    "Invalid Dates", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                projectManager.AddTask(taskName);

                if (!string.IsNullOrWhiteSpace(parentTaskTextBox.Text))
                {
                    projectManager.AddSubtaskToTask(taskName, parentTaskTextBox.Text);
                }

                if (!string.IsNullOrWhiteSpace(descriptionTextBox.Text))
                {
                    projectManager.AddDescriptionToTask(taskName, descriptionTextBox.Text);
                }

                projectManager.SetTimelineOfTask(taskName, 
                    startDatePicker.Value, endDatePicker.Value);
                
                projectManager.UpdateStatusOfTask(taskName, statusComboBox.Text);
                projectManager.SetPriorityOfTask(taskName, priorityComboBox.Text);

                if (float.TryParse(timePerDayTextBox.Text, out float hours))
                {
                    projectManager.SetNumberOfWorkingHoursPerDayOfTask(taskName, hours);
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void endDatePicker_ValueChanged(object sender, EventArgs e)
        {
            if (startDatePicker.Value >= endDatePicker.Value)
            {
                MessageBox.Show("End date must be later than start date!", 
                    "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                endDatePicker.Value = startDatePicker.Value.AddDays(1);
            }
        }

        private void startDatePicker_ValueChanged(object sender, EventArgs e)
        {
            if (startDatePicker.Value >= endDatePicker.Value)
            {
                MessageBox.Show("Start date must be earlier than end date!", 
                    "Invalid Date", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                startDatePicker.Value = endDatePicker.Value.AddDays(-1);
            }
        }
    }
}