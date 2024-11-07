using System;
using System.Security.Policy;
using System.Windows.Forms;

namespace Project_Manager_Pro.GUI;

internal partial class RSform : Form
{
    private string projectName;
    private DataGridView resourceViewTable;
    private Button addWorkRsBtn, addMaterialRsBtn, editResourceBtn, viewResourceInfoBtn;
    private ProjectManagement projectManager;

    public RSform(string projectName, ProjectManagement projectManager)
    {
        this.projectName = projectName;
        this.projectManager = projectManager;
        InitializeRSform();
    }

    private void InitializeRSform()
    {
        this.Text = $"Resource Manager: {projectName}";
        this.Size = new Size(1200, 600);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;

        // Initialize DataGridView
        resourceViewTable = new DataGridView
        {
            Location = new Point(20, 20),
            Size = new Size(1100, 400),
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            ReadOnly = true
        };

        // Add columns
        resourceViewTable.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn { Name = "ResourceName", HeaderText = "Resource Name" },
            new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type" },
            new DataGridViewTextBoxColumn { Name = "MaxCapacity", HeaderText = "Max Capacity" },
            new DataGridViewTextBoxColumn { Name = "StdRate", HeaderText = "Std. Rate" },
            new DataGridViewTextBoxColumn { Name = "OvertimeRate", HeaderText = "Overtime Rate" },
            new DataGridViewTextBoxColumn { Name = "Cost", HeaderText = "Cost" },
            new DataGridViewTextBoxColumn { Name = "Accrue", HeaderText = "Accrue" }
        });

        // Buttons
        addWorkRsBtn = new Button
        {
            Text = "Add Work Resource",
            Location = new Point(20, 440),
            Size = new Size(150, 30)
        };

        addMaterialRsBtn = new Button
        {
            Text = "Add Material Resource",
            Location = new Point(190, 440),
            Size = new Size(150, 30)
        };

        editResourceBtn = new Button
        {
            Text = "Edit Resource",
            Location = new Point(360, 440),
            Size = new Size(150, 30)
        };

    //    viewResourceInfoBtn = new Button
    //    {
    //        Text = "View Resource Info",
    //        Location = new Point(530, 440),
    //        Size = new Size(150, 30)
    //    };

        // Add controls
        this.Controls.AddRange(new Control[]
        {
            resourceViewTable,
            addWorkRsBtn,
            addMaterialRsBtn,
            editResourceBtn,
    //        viewResourceInfoBtn
        });

        // Add event handlers
        addWorkRsBtn.Click += AddWorkRsBtn_Click;
        addMaterialRsBtn.Click += AddMaterialRsBtn_Click;
        editResourceBtn.Click += EditResourceBtn_Click;
    //    viewResourceInfoBtn.Click += ViewResourceInfoBtn_Click;

        RefreshResourceView();
    }

    private void AddWorkRsBtn_Click(object sender, EventArgs e)
    {
        using (var form = new AddResourceForm("Work"))
        {
            if (form.ShowDialog() == DialogResult.OK)
            {
                projectManager.AddWorkResource(form.ResourceName);
                RefreshResourceView();
            }
        }
    }

    private void AddMaterialRsBtn_Click(object sender, EventArgs e)
    {
        using (var form = new AddResourceForm("Material"))
        {
            if (form.ShowDialog() == DialogResult.OK)
            {
                projectManager.AddMaterialResource(form.ResourceName);
                RefreshResourceView();
            }
        }
    }

    private void EditResourceBtn_Click(object sender, EventArgs e)
    {
        if (resourceViewTable.SelectedRows.Count > 0)
        {
            string resourceName = resourceViewTable.SelectedRows[0].Cells["ResourceName"].Value.ToString();
            using (var form = new EditResourceForm(resourceName, projectManager))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    RefreshResourceView();
                }
            }
        }
    }

/*    private void ViewResourceInfoBtn_Click(object sender, EventArgs e)
    {
        if (resourceViewTable.SelectedRows.Count > 0)
        {
            string resourceName = resourceViewTable.SelectedRows[0].Cells["ResourceName"].Value.ToString();
            using (var form = new TaskResourceInfo(resourceName, projectManager))
            {
                form.ShowDialog();
            }
        }
        else
        {
            MessageBox.Show("Please select a resource first.", "No Resource Selected", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }*/

    private void RefreshResourceView()
    {
        resourceViewTable.Rows.Clear();
        
        // Add work resources
        foreach (var workResource in projectManager.Resources.WorkResourceList)
        {
            resourceViewTable.Rows.Add(
                workResource.Value.ResourceName,
                "Work",
                workResource.Value.AvailableCapacity,
                workResource.Value.StandardRate,
                workResource.Value.OvertimeRate,
                "", // Cost will be calculated
                workResource.Value.Accrue
            );
        }

        // Add material resources
        foreach (var materialResource in projectManager.Resources.MaterialResourceList)
        {
            resourceViewTable.Rows.Add(
                materialResource.Value.ResourceName,
                "Material",
                1,
                materialResource.Value.StandardRate,
                "", // Overtime rate not applicable for material resources
                "", // Cost will be calculated
                "End" // Default accrue type for material resources
            );
        }
    }
}