using System;
using System.Windows.Forms;

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

            PMform mainForm = new PMform(projectName, currentDatePicker.Value);

            this.Hide();
            mainForm.FormClosed += (s, args) => this.Close();
            mainForm.Show();
        }
    }
}
