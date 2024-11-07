using System.Windows.Forms;

namespace Project_Manager_Pro.GUI
{
    internal class EditResourceForm : Form
    {
        private TextBox nameTextBox;
        private TextBox currencyTextBox;
        private Button saveButton;
        private Button deleteButton;
        private ComboBox resourceTypeComboBox;
        private ProjectManagement projectManager;
        private string currentResourceName;
        private TextBox capacityTextBox;

        public EditResourceForm(string resourceName, ProjectManagement projectManager)
        {
            this.projectManager = projectManager;
            this.currentResourceName = resourceName;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = $"Edit Resource: {currentResourceName}";
            this.Size = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // Resource Type ComboBox
            var typeLabel = new Label { Text = "Resource Type:", Location = new Point(20, 20) };
            resourceTypeComboBox = new ComboBox
            {
                Location = new Point(150, 20),
                Size = new Size(200, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            resourceTypeComboBox.Items.AddRange(new string[] { "Work", "Material" });
            resourceTypeComboBox.SelectedItem = "Work"; // Default selection

            // Name TextBox
            var nameLabel = new Label { Text = "New Resource Name:", Location = new Point(20, 60) };
            nameTextBox = new TextBox
            {
                Location = new Point(150, 60),
                Size = new Size(200, 25),
                Text = currentResourceName
            };

            // Currency TextBox
            var currencyLabel = new Label { Text = "Currency:", Location = new Point(20, 100) };
            currencyTextBox = new TextBox
            {
                Location = new Point(150, 100),
                Size = new Size(200, 25)
            };

            // Capacity TextBox
            var capacityLabel = new Label { Text = "Capacity:", Location = new Point(20, 140) };
            capacityTextBox = new TextBox
            {
                Location = new Point(150, 140),
                Size = new Size(200, 25)
            };

            // Save Button
            saveButton = new Button
            {
                Text = "Save",
                DialogResult = DialogResult.OK,
                Location = new Point(270, 200),
                Size = new Size(80, 30)
            };
            saveButton.Click += SaveButton_Click;

            // Delete Button
            deleteButton = new Button
            {
                Text = "Delete Resource",
                Location = new Point(20, 200),
                Size = new Size(120, 30),
                ForeColor = Color.Red
            };
            deleteButton.Click += DeleteButton_Click;

            // Add controls
            this.Controls.AddRange(new Control[]
            {
                typeLabel, resourceTypeComboBox,
                nameLabel, nameTextBox,
                currencyLabel, currencyTextBox,
                capacityLabel, capacityTextBox,
                saveButton, deleteButton
            });
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Resource name cannot be empty.", "Invalid Input", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (resourceTypeComboBox.SelectedItem.ToString() == "Work")
            {
                projectManager.ChangeNameOfWorkResource(currentResourceName, nameTextBox.Text);
            }
            else
            {
                projectManager.ChangeNameOfMaterialResource(currentResourceName, nameTextBox.Text);
            }

            if (!string.IsNullOrWhiteSpace(currencyTextBox.Text))
            {
                projectManager.SetCurrencyForAllResource(currencyTextBox.Text);
            }

            // Add capacity validation
            if (int.TryParse(capacityTextBox.Text, out int capacity))
            {
                if (capacity < 1 || capacity > 100)
                {
                    MessageBox.Show("Capacity must be between 1 and 100.", "Invalid Input", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                projectManager.SetAvailableCapacityOfWorkResource(currentResourceName, capacity);
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the resource '{currentResourceName}'?",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                if (resourceTypeComboBox.SelectedItem.ToString() == "Work")
                {
                    projectManager.DeleteWorkResource(currentResourceName);
                }
                else
                {
                    projectManager.DeleteMaterialResource(currentResourceName);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
} 