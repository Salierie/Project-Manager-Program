using System;
using System.Drawing;
using System.Windows.Forms;

namespace Project_Manager_Pro.GUI
{
    internal partial class TaskResourceInfo : Form
    {
        private readonly string taskName;
        private readonly ProjectManagement projectManager;
        private DataGridView resourceGrid;
        private Label totalCostLabel;

        public TaskResourceInfo(string taskName, ProjectManagement projectManager)
        {
            this.taskName = taskName;
            this.projectManager = projectManager;
            InitializeTaskResourceInfo();
        }

        private void InitializeTaskResourceInfo()
        {
            // Form settings
            this.Text = $"Task Resource Information: {taskName}";
            this.Size = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Total Cost Label
            totalCostLabel = new Label
            {
                Text = "Total Cost: Calculating...",
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 10, FontStyle.Bold)
            };

            // Resource Grid
            resourceGrid = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(540, 280),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = true
            };

            // Add columns
            resourceGrid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn 
                { 
                    Name = "ResourceName",
                    HeaderText = "Resource Name",
                    Width = 150
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Capacity",
                    HeaderText = "Capacity",
                    Width = 80
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Type",
                    HeaderText = "Type",
                    Width = 80
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Accrue",
                    HeaderText = "Accrue",
                    Width = 80
                },
                new DataGridViewTextBoxColumn 
                { 
                    Name = "Cost",
                    HeaderText = "Cost",
                    Width = 100
                }
            });

            // Add controls to form
            this.Controls.Add(totalCostLabel);
            this.Controls.Add(resourceGrid);

            // Load resource data
            LoadResourceData();
        }

        private void LoadResourceData()
        {
            resourceGrid.Rows.Clear();
            var task = projectManager.ProjectTree.FindTaskNode(taskName);
            
            if (task == null) return;

            foreach (var resource in task.ResourceAndCapacityDic)
            {
                string resourceName = resource.Key;
                int capacity = resource.Value;
                string type = "";
                string accrue = "";
                string cost = "";

                if (projectManager.Resources.CheckIfWorkResourceExists(resourceName))
                {
                    var workResource = projectManager.Resources.WorkResourceList[resourceName];
                    type = "Work";
                    accrue = workResource.Accrue;
                    // Cost calculation would be based on your business logic
                }
                else if (projectManager.Resources.CheckIfMaterialResourceExists(resourceName))
                {
                    var materialResource = projectManager.Resources.MaterialResourceList[resourceName];
                    type = "Material";
                    accrue = "End";
                    // Cost calculation would be based on your business logic
                }

                resourceGrid.Rows.Add(resourceName, capacity, type, accrue, cost);
            }

            // Update total cost
            UpdateTotalCost();
        }

        private void UpdateTotalCost()
        {
            var task = projectManager.ProjectTree.FindTaskNode(taskName);
            if (task == null) return;

            decimal totalCost = 0;
            foreach (var resource in task.ResourceAndCapacityDic)
            {
                string resourceName = resource.Key;
                int capacity = resource.Value;
                
                if (projectManager.Resources.CheckIfWorkResourceExists(resourceName))
                {
                    var workResource = projectManager.Resources.WorkResourceList[resourceName];
                    totalCost += (decimal)(workResource.StandardRate * capacity);
                }
                else if (projectManager.Resources.CheckIfMaterialResourceExists(resourceName))
                {
                    var materialResource = projectManager.Resources.MaterialResourceList[resourceName];
                    totalCost += (decimal)materialResource.StandardRate * capacity;
                }
            }

            totalCostLabel.Text = $"Total Cost: {totalCost:C}";
        }
    }
}
