using System;
using System.Threading;
using System.Windows.Forms;
using Project_Manager_Pro.GUI;
namespace Project_Manager_Pro
{
    internal static class TestCase
    {
        public static void RunTest()
        {
            Console.WriteLine("Starting application test with sample data...\n");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Create project with sample data
            string projectName = "Sample Project";
            DateTime projectDate = DateTime.Now;
            var projectManager = new ProjectManagement(projectName);
            projectManager.SetCurrenDateOfProject(projectDate);

            // Add sample tasks with various properties
            Console.WriteLine("1. Creating sample tasks...");
            AddSampleTasks(projectManager);

            // Add sample resources
            Console.WriteLine("\n2. Creating sample resources...");
            AddSampleResources(projectManager);

            // Assign resources to tasks
            Console.WriteLine("\n3. Assigning resources to tasks...");
            AssignResourcesToTasks(projectManager);

            Console.WriteLine("\nTest data setup completed!");
            Console.WriteLine("----------------------------------------\n");

            // Start the application
            var welcomeForm = new FormWelcome();
            Application.Run(welcomeForm);
        }

        public static void AddSampleTasks(ProjectManagement projectManager)
        {
            // Main tasks
            projectManager.AddTask("Project Planning");
            projectManager.AddTask("Design Phase");
            projectManager.AddTask("Development");
            projectManager.AddTask("Testing");
            projectManager.AddTask("Deployment");

            // Subtasks
            projectManager.AddSubtaskToTask("Requirements Analysis", "Project Planning");
            projectManager.AddSubtaskToTask("Resource Allocation", "Project Planning");
            projectManager.AddSubtaskToTask("Project Schedule", "Project Planning");

            projectManager.AddSubtaskToTask("UI/UX Design", "Design Phase");
            projectManager.AddSubtaskToTask("Database Design", "Design Phase");
            projectManager.AddSubtaskToTask("Architecture Design", "Design Phase");

            projectManager.AddSubtaskToTask("Frontend Development", "Development");
            projectManager.AddSubtaskToTask("Backend Development", "Development");
            projectManager.AddSubtaskToTask("Database Implementation", "Development");

            projectManager.AddSubtaskToTask("Unit Testing", "Testing");
            projectManager.AddSubtaskToTask("Integration Testing", "Testing");
            projectManager.AddSubtaskToTask("User Acceptance Testing", "Testing");

            projectManager.AddSubtaskToTask("Environment Setup", "Deployment");
            projectManager.AddSubtaskToTask("Data Migration", "Deployment");
            projectManager.AddSubtaskToTask("Go Live", "Deployment");

            // Set timelines for tasks
            DateTime startDate = DateTime.Now;
            
            // Project Planning phase (Completed)
            projectManager.SetTimelineOfTask("Requirements Analysis", startDate, startDate.AddDays(5));
            projectManager.SetTimelineOfTask("Resource Allocation", startDate.AddDays(3), startDate.AddDays(7));
            projectManager.SetTimelineOfTask("Project Schedule", startDate.AddDays(6), startDate.AddDays(10));
            projectManager.SetTimelineOfTask("Project Planning", startDate, startDate.AddDays(10));

            // Design Phase (In Progress)
            projectManager.SetTimelineOfTask("UI/UX Design", startDate.AddDays(11), startDate.AddDays(20));
            projectManager.SetTimelineOfTask("Database Design", startDate.AddDays(15), startDate.AddDays(25));
            projectManager.SetTimelineOfTask("Architecture Design", startDate.AddDays(15), startDate.AddDays(25));
            projectManager.SetTimelineOfTask("Design Phase", startDate.AddDays(11), startDate.AddDays(25));

            // Development (Not Started)
            projectManager.SetTimelineOfTask("Frontend Development", startDate.AddDays(26), startDate.AddDays(40));
            projectManager.SetTimelineOfTask("Backend Development", startDate.AddDays(26), startDate.AddDays(45));
            projectManager.SetTimelineOfTask("Database Implementation", startDate.AddDays(30), startDate.AddDays(45));
            projectManager.SetTimelineOfTask("Development", startDate.AddDays(26), startDate.AddDays(45));

            // Testing (Not Started)
            projectManager.SetTimelineOfTask("Unit Testing", startDate.AddDays(35), startDate.AddDays(50));
            projectManager.SetTimelineOfTask("Integration Testing", startDate.AddDays(46), startDate.AddDays(55));
            projectManager.SetTimelineOfTask("User Acceptance Testing", startDate.AddDays(51), startDate.AddDays(60));
            projectManager.SetTimelineOfTask("Testing", startDate.AddDays(35), startDate.AddDays(60));

            // Deployment (Not Started)
            projectManager.SetTimelineOfTask("Environment Setup", startDate.AddDays(56), startDate.AddDays(65));
            projectManager.SetTimelineOfTask("Data Migration", startDate.AddDays(61), startDate.AddDays(70));
            projectManager.SetTimelineOfTask("Go Live", startDate.AddDays(71), startDate.AddDays(75));
            projectManager.SetTimelineOfTask("Deployment", startDate.AddDays(56), startDate.AddDays(75));

            // Set task statuses and completion percentages
            projectManager.UpdateStatusOfTask("Requirements Analysis", "Complete");
            projectManager.UpdateStatusOfTask("Resource Allocation", "Complete");
            projectManager.UpdateStatusOfTask("Project Schedule", "Complete");
            projectManager.UpdateStatusOfTask("Project Planning", "Complete");

            projectManager.UpdateStatusOfTask("UI/UX Design", "Complete");
            projectManager.UpdateStatusOfTask("Database Design", "In Progress");
            projectManager.UpdateStatusOfTask("Architecture Design", "In Progress");
            projectManager.UpdateStatusOfTask("Design Phase", "In Progress");

            // Set priorities
            projectManager.SetPriorityOfTask("Frontend Development", "High");
            projectManager.SetPriorityOfTask("Backend Development", "High");
            projectManager.SetPriorityOfTask("Database Implementation", "Medium");
            projectManager.SetPriorityOfTask("Unit Testing", "Medium");
            projectManager.SetPriorityOfTask("Integration Testing", "High");
            projectManager.SetPriorityOfTask("User Acceptance Testing", "High");
            projectManager.SetPriorityOfTask("Go Live", "High");

            // Add dependencies
            projectManager.AddDependency("Resource Allocation", "Requirements Analysis", "FS");
            projectManager.AddDependency("Project Schedule", "Resource Allocation", "FS");
            projectManager.AddDependency("UI/UX Design", "Project Planning", "FS");
            projectManager.AddDependency("Database Design", "Project Planning", "FS");
            projectManager.AddDependency("Architecture Design", "Project Planning", "FS");
            projectManager.AddDependency("Frontend Development", "UI/UX Design", "FS");
            projectManager.AddDependency("Backend Development", "Architecture Design", "FS");
            projectManager.AddDependency("Database Implementation", "Database Design", "FS");
            projectManager.AddDependency("Unit Testing", "Development", "SS");
            projectManager.AddDependency("Integration Testing", "Unit Testing", "FS");
            projectManager.AddDependency("User Acceptance Testing", "Integration Testing", "FS");
            projectManager.AddDependency("Environment Setup", "Testing", "FS");
            projectManager.AddDependency("Data Migration", "Environment Setup", "FS");
            projectManager.AddDependency("Go Live", "Data Migration", "FS");
        }

        public static void AddSampleResources(ProjectManagement projectManager)
        {
            // Add work resources
            var workResources = new[] {
                ("Project Manager", 100.0f, 150.0f),
                ("Senior Developer", 80.0f, 120.0f),
                ("Developer", 60.0f, 90.0f),
                ("Designer", 70.0f, 105.0f),
                ("Tester", 50.0f, 75.0f)
            };

            foreach (var (name, stdRate, otRate) in workResources)
            {
                projectManager.AddWorkResource(name);
                projectManager.SetStandardRateOfWorkResource(name, stdRate);
                projectManager.SetOvertimeRateOfWorkResource(name, otRate);
                projectManager.SetMaximumWorkingHoursPerDayOfWorkResource(name, 8);
                projectManager.SetAccrueTypeOfWorkResource(name, GetRandomAccrueType());
            }

            // Add material resources
            var materialResources = new[] {
                ("Software Licenses", 500.0f, 5),
                ("Development Tools", 200.0f, 3),
                ("Cloud Services", 100.0f, 2),
                ("Hardware Equipment", 1000.0f, 1)
            };

            foreach (var (name, rate, capacity) in materialResources)
            {
                projectManager.AddMaterialResource(name);
                projectManager.SetStandardRateOfMaterialResource(name, rate);
                projectManager.SetAvailableCapacityOfWorkResource(name, capacity);
            }
        }

        public static void AssignResourcesToTasks(ProjectManagement projectManager)
        {
            // Assign work resources
            var assignments = new[] {
                ("Project Planning", "Project Manager", 1),
                ("Design Phase", "Designer", 2),
                ("Development", "Senior Developer", 1),
                ("Development", "Developer", 2),
                ("Testing", "Tester", 2),
                ("Deployment", "Senior Developer", 1)
            };

            foreach (var (task, resource, capacity) in assignments)
            {
                projectManager.AddResourceToTask(resource, task);
                projectManager.AddCapacityToResourceOfTask(resource, task, capacity);
            }

            // Assign material resources
            var materialAssignments = new[] {
                ("Development", "Software Licenses", 5),
                ("Development", "Development Tools", 3),
                ("Deployment", "Cloud Services", 2),
                ("Testing", "Hardware Equipment", 1)
            };

            foreach (var (task, resource, capacity) in materialAssignments)
            {
                projectManager.AddResourceToTask(resource, task);
                projectManager.AddCapacityToResourceOfTask(resource, task, capacity);
            }
        }

        private static string GetRandomStatus()
        {
            var statuses = new[] { "Not Start", "In Progress", "Complete" };
            return statuses[new Random().Next(statuses.Length)];
        }

        private static string GetRandomPriority()
        {
            var priorities = new[] { "Low", "Medium", "High" };
            return priorities[new Random().Next(priorities.Length)];
        }

        private static string GetRandomAccrueType()
        {
            var accrueTypes = new[] { "Start", "End", "Prorated" };
            return accrueTypes[new Random().Next(accrueTypes.Length)];
        }
    }
} 