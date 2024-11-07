using System;
using System.Windows.Forms;

namespace Project_Manager_Pro.GUI
{
    internal partial class EditTaskRSForm : Form{
        public string? taskName;
        public string? resourceName;
        private TextBox resourceNameTextBox;
        private TextBox resourceCapacityTextBox;
        private Button deleteResourceBtn;
        private Button deleteAllResourceBtn;
        private Button saveBtn;
        private GroupBox capacityGroupBox;
        private ProjectManagement projectManager;

        public EditTaskRSForm(string taskName, ProjectManagement projectManager)
        {
            this.taskName = taskName;
            this.projectManager = projectManager;
            InitializeTRSEdit();
        }
        private void InitializeTRSEdit(){
            // Form setup
            this.Text = "Edit Task Resource";
            this.Size = new Size(500, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Resource Name section
            Label resourceNameLabel = new Label
            {
                Text = "Enter Resource Name:",
                Location = new Point(20, 20),
                Size = new Size(150, 20)
            };

            resourceNameTextBox = new TextBox
            {
                Location = new Point(20, 45),
                Size = new Size(300, 20)
            };

            // Capacity GroupBox
            capacityGroupBox = new GroupBox
            {
                Text = "",
                Location = new Point(20, 85),
                Size = new Size(440, 180)
            };

            Label capacityLabel = new Label
            {
                Text = "Enter Resource Capacity:",
                Location = new Point(10, 30),
                Size = new Size(150, 20)
            };

            resourceCapacityTextBox = new TextBox
            {
                Location = new Point(10, 60),
                Size = new Size(300, 20)
            };

            deleteResourceBtn = new Button
            {
                Text = "Delete Resource",
                Location = new Point(10, 100),
                Size = new Size(150, 30)
            };

            // Add controls to capacity GroupBox
            capacityGroupBox.Controls.AddRange(new Control[]
            {
                capacityLabel,
                resourceCapacityTextBox,
                deleteResourceBtn
            });

            // Bottom buttons
            deleteAllResourceBtn = new Button
            {
                Text = "Delete All Resource",
                Location = new Point(20, 280),
                Size = new Size(150, 30)
            };

            saveBtn = new Button
            {
                Text = "Save",
                Location = new Point(360, 280),
                Size = new Size(100, 30)
            };

            // Add all controls to form
            this.Controls.AddRange(new Control[]
            {
                resourceNameLabel,
                resourceNameTextBox,
                capacityGroupBox,
                deleteAllResourceBtn,
                saveBtn
            });

            // Add event handlers
            saveBtn.Click += SaveBtn_Click;
            deleteResourceBtn.Click += DeleteResourceBtn_Click;
            deleteAllResourceBtn.Click += DeleteAllResourceBtn_Click;
            resourceNameTextBox.TextChanged += ResourceNameTextBox_TextChanged;
        }

        private void ResourceNameTextBox_TextChanged(object sender, EventArgs e)
        {
            resourceName = resourceNameTextBox.Text;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(resourceNameTextBox.Text))
            {
                MessageBox.Show("Please enter a resource name.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (int.TryParse(resourceCapacityTextBox.Text, out int capacity))
            {
                projectManager.AddResourceToTask(resourceNameTextBox.Text, taskName);
                projectManager.AddCapacityToResourceOfTask(resourceNameTextBox.Text, taskName, capacity);
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid capacity number.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void DeleteResourceBtn_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(resourceNameTextBox.Text))
            {
                DialogResult result = MessageBox.Show(
                    "Are you sure you want to delete this resource?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    projectManager.DeleteResourceOfTask(resourceNameTextBox.Text, taskName);
                    resourceNameTextBox.Clear();
                    resourceCapacityTextBox.Clear();
                }
            }
        }

        private void DeleteAllResourceBtn_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete all resources? This action cannot be undone.",
                "Confirm Delete All",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                projectManager.DeleteAllResourceOfTask(taskName);
                resourceNameTextBox.Clear();
                resourceCapacityTextBox.Clear();
            }
        }        

        internal void SetTaskAndProject(string taskName, ProjectManagement projectManager)
        {
            this.taskName = taskName;
            this.projectManager = projectManager;
        }
    }


}


