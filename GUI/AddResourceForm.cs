using System.Windows.Forms;

namespace Project_Manager_Pro.GUI
{
    public class AddResourceForm : Form
    {
        public string ResourceName { get; private set; }
        private TextBox nameTextBox;
        private string resourceType;

        public AddResourceForm(string type)
        {
            resourceType = type;
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Form properties
            this.Text = $"Add {resourceType} Resource";
            this.Size = new Size(300, 150);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            // Label
            Label nameLabel = new Label
            {
                Text = "Resource Name:",
                Location = new Point(20, 20),
                Size = new Size(100, 20)
            };
            
            // TextBox
            nameTextBox = new TextBox
            {
                Location = new Point(120, 20),
                Size = new Size(150, 20)
            };
            
            // Buttons
            Button okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(120, 60),
                Size = new Size(70, 30)
            };
            
            Button cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(200, 60),
                Size = new Size(70, 30)
            };
            
            okButton.Click += (s, e) => ResourceName = nameTextBox.Text;
            
            // Add controls
            Controls.AddRange(new Control[] { nameLabel, nameTextBox, okButton, cancelButton });
        }
    }
} 