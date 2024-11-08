using System;
using System.Collections.Generic;
using System.Net;
using System.Resources;
using System.Runtime.ConstrainedExecution;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Project_Manager_Pro.GUI;
using System.Text.Json;
using System.IO;

namespace Project_Manager_Pro
{
    abstract class Resource
    {
        public string ResourceName = "";
        public float StandardRate; //>= 0
        public string Currency = "$"; //VND, Euro,...

        protected Resource() { }

        public abstract void PrintInfo();

        public void Print(string text)
        {
            Console.Write(text);
        }
    }

    class WorkResource : Resource
    {
        public float OvertimeRate; //>= 0
        private const string RateUnit = "hour";
        public float MaximumWorkingHoursPerDay = 24; //>= 0 && <= 24
        public int AvailableCapacity = 1; //>= 1
        public string Accrue = "Prorated"; //Start/Prorated/End

        public WorkResource(string name)
        {
            ResourceName = name;
        }

        public override void PrintInfo()
        {
            Print($"Resource name: {ResourceName}\n");
            Print($"Standard rate cost: {StandardRate} {Currency}/{RateUnit}\n");
            Print($"Overtime rate cost: {OvertimeRate} {Currency}/{RateUnit}\n");
            Print($"Maximum working hours: {MaximumWorkingHoursPerDay} hours/day\n");
            Print($"Available capacity: {AvailableCapacity}\n");
            Print($"Accrue type: {Accrue}\n");
        }
    }

    class MaterialResource : Resource
    {
        public string RateUnit = "";

        public MaterialResource(string name)
        {
            ResourceName = name;
        }

        public override void PrintInfo()
        {
            Print($"Resource name: {ResourceName}\n");
            Print($"Standard rate cost: {StandardRate} {Currency}/{RateUnit}\n");
        }
    }

    class ResourceManagement
    {
        public string Currency = "$";
        public Dictionary<string, WorkResource> WorkResourceList = new Dictionary<string, WorkResource>();
        public Dictionary<string, MaterialResource> MaterialResourceList = new Dictionary<string, MaterialResource>();

        public ResourceManagement() { }

        public void AddNewWorkResource(string name)
        {
            if (name.Length > 20) Print($"The name's length of {name} is over 20 characters!\n");
            else if (!CheckIfWorkResourceExists(name) && !CheckIfMaterialResourceExists(name))
            {
                WorkResourceList.Add(name, new WorkResource(name));
                WorkResourceList[name].Currency = Currency;
            }
            else Print($"Resource with name {name} already exists!\n");
        }

        public void AddNewMaterialResource(string name)
        {
            if (name.Length > 20) Print($"The name's length of {name} is over 20 characters!\n");
            else if (!CheckIfMaterialResourceExists(name) && !CheckIfWorkResourceExists(name))
            {
                MaterialResourceList.Add(name, new MaterialResource(name));
                MaterialResourceList[name].Currency = Currency;
            }
            else Print($"Resource with name {name} already exists!\n");
        }

        public void ChangeWorkResourceName(string currentName, string newName)
        {
            if (newName.Length > 20) Print($"The name's length of {newName} is over 20 characters!\n");
            else if (CheckIfWorkResourceExists(currentName) && !CheckIfWorkResourceExists(newName))
            {
                WorkResourceList.Add(newName, WorkResourceList[currentName]);
                WorkResourceList[newName].ResourceName = newName;
                WorkResourceList.Remove(currentName);
            }
            else Print($"Cannot change because the current name {currentName} does not exist or the new name {newName} already exists!\n");
        }

        public void ChangeMaterialResourceName(string currentName, string newName)
        {
            if (newName.Length > 20) Print($"The name's length of {newName} is over 20 characters!\n");
            else if (CheckIfMaterialResourceExists(currentName) && !CheckIfMaterialResourceExists(newName))
            {
                MaterialResourceList.Add(newName, MaterialResourceList[currentName]);
                MaterialResourceList[newName].ResourceName = newName;
                MaterialResourceList.Remove(currentName);
            }
            else Print($"Cannot change because the current name {currentName} does not exist or the new name {newName} already exists!\n");
        }

        public void DeleteWorkResource(string name)
        {
            if (CheckIfWorkResourceExists(name)) WorkResourceList.Remove(name);
            else Print($"Cannot delete resource {name} because it does not exist!\n");
        }

        public void DeleteMaterialResource(string name)
        {
            if (CheckIfMaterialResourceExists(name)) MaterialResourceList.Remove(name);
            else Print($"Cannot delete resource {name} because it does not exist!\n");
        }

        public void SetStandardRateOfWorkResource(string name, float StdRate)
        {
            if (CheckIfWorkResourceExists(name) && StdRate >= 0) WorkResourceList[name].StandardRate = StdRate;
            else Print($"Cannot set standard rate because the resource {name} does not exist or the value of standatd rate is less than 0!\n");
        }

        public void SetStandardRateOfMaterialResource(string name, float StdRate)
        {
            if (CheckIfMaterialResourceExists(name) && StdRate >= 0) MaterialResourceList[name].StandardRate = StdRate;
            else Print($"Cannot set standard rate because the resource {name} does not exist or the value of standatd rate is less than 0!\n");
        }

        public void SetOvertimeRateOfWorkResource(string name, float OvertimeRate)
        {
            if (CheckIfWorkResourceExists(name) && OvertimeRate >= 0) WorkResourceList[name].OvertimeRate = OvertimeRate;
            else Print($"Cannot set overtime rate because the resource {name} does not exist or the value of overtime rate is less than 0!\n");
        }

        public void SetCurrencyForAllResources(string Currency)
        {
            this.Currency = Currency;

            foreach(KeyValuePair<string, WorkResource> resource in WorkResourceList)
            {
                resource.Value.Currency = Currency;
            }

            foreach(KeyValuePair<string, MaterialResource> resource in MaterialResourceList)
            {
                resource.Value.Currency = Currency;
            }
        }

        public void SetMaximumWorkingHoursPerDayOfWorkResource(string name, float hours)
        {
            if (CheckIfWorkResourceExists(name) && hours >= 0 && hours <= 24) WorkResourceList[name].MaximumWorkingHoursPerDay = hours;
            else Print($"Cannot set maximum working hours per day because the resource {name} does not exist or the value of hours is outside of range [0, 24]!\n");
        }

        public void SetAvailableCapacityOfWorkResource(string name, int capacity)
        {
            if (CheckIfWorkResourceExists(name) && capacity >= 1) WorkResourceList[name].AvailableCapacity = capacity;
            else Print($"Cannot set capacity because the resource {name} does not exist or the value of capacity is less than 1!\n");
        }

        public void SetRateUnitOfMaterialResource(string name, string RateUnit)
        {
            if (CheckIfMaterialResourceExists(name)) MaterialResourceList[name].RateUnit = RateUnit;
            else Print($"Cannot set rate unit because the reource {name} does not exist!\n");
        }

        public void SetAccrueOfWorkResource(string name, string AccrueType)
        {
            if (CheckIfWorkResourceExists(name) && (AccrueType == "Start" || AccrueType == "Prorated" || AccrueType == "End")) WorkResourceList[name].Accrue = AccrueType;
            else if (!CheckIfWorkResourceExists(name)) Print($"Cannot set accure because the resource {name} does not exist!\n");
            else Print("Please choose the accrue type: Start/Prorated/End\n");
        }

        public bool CheckIfWorkResourceExists(string name)
        {
            return WorkResourceList.ContainsKey(name);
        }

        public bool CheckIfMaterialResourceExists(string name)
        {
            return MaterialResourceList.ContainsKey(name);
        }

        public void PrintInfoOfWorkResource(string name)
        {
            if (WorkResourceList.ContainsKey(name)) WorkResourceList[name].PrintInfo();
            else Print($"Cannot print info because the resource {name} does not exist!\n");
        }

        public void PrintInfoOfMaterialResource(string name)
        {
            if (MaterialResourceList.ContainsKey(name)) MaterialResourceList[name].PrintInfo();
            else Print($"Cannot print info because the resource {name} does not exist!\n");
        }

        private void Print(string text)
        {
            Console.Write(text);
        }
    }

    class TaskIDandSubtasksID
    {
        public string TaskID;
        public int latestPositionID = 0;
        public List<string> SubTaskIDS = new List<string>();

        public TaskIDandSubtasksID(string ID)
        {
            TaskID = ID;
        }
    }

    class Task
    {
        public string TaskID;
        public string TaskName;
        public List<string> Desription = new List<string>();
        public string Priority = "";

        public Task(string taskID, string taskName)
        {
            TaskID = taskID;
            TaskName = taskName;
        }

        public int TaskNodeLevelInTree;
        public DateTime StartDate = DateTime.MinValue;
        public DateTime EndDate = DateTime.MinValue;
        public int Duration = 1;
        public float TaskWorkingHoursPerDay; //In hours >= 0 && <= 24
        public string Status = "Not start";
        public int PercentageCompleted = 0;
        public bool IsLeafNode = true;

        public Dictionary<string, int> ResourceAndCapacityDic = new Dictionary<string, int>();
        public Task? ParentTask;
        public Dictionary<string, Task> SubTasks = new Dictionary<string, Task>();
    }

    class TypeLag
    {
        public string Type;
        public int Lag = 0;

        public TypeLag(string type)
        {
            Type = type;
        }
    }

    class DependencyTaskInfo
    {
        public string TaskName;
        public Dictionary<string, TypeLag> DependingTasks = new Dictionary<string, TypeLag>();

        public DependencyTaskInfo(string taskName)
        {
            TaskName = taskName;
        }
    }

    interface IPrintWordWithEmptySpace
    {
        void PrintWordWithEmptySpace(string word, int MaximumEmptySpace);
    }

    class TreeOfTasks : IPrintWordWithEmptySpace
    {
        public Task RootTask;
        private int latestLevelOneTaskID = 0;
        public DateTime CurrentDate = DateTime.Now;
        public PDMDiGraph graph = new PDMDiGraph();
        public Dictionary<string, string> TaskNameandIDDic = new Dictionary<string, string>();
        public Dictionary<string, TaskIDandSubtasksID> TaskIDandSubTaskIDDic = new Dictionary<string, TaskIDandSubtasksID>();
        public Dictionary<string, DependencyTaskInfo> Dependencies = new Dictionary<string, DependencyTaskInfo>();

        public TreeOfTasks(string ProjectName)
        {
            RootTask = new Task("0", ProjectName);
            RootTask.IsLeafNode = false;
            TaskNameandIDDic.Add(ProjectName, "0");
            TaskIDandSubTaskIDDic.Add("0", new TaskIDandSubtasksID("0"));
        }

        public void ChangeTaskName(string TaskName, string newTaskName)
        {
            if (newTaskName.Length > 30)
            {
                Print("The name's length is over 30 characters!\n");
                return;
            }
            else if (AlreadyHaveThisTask(TaskName) && !AlreadyHaveThisTask(newTaskName))
            {
                Task? Task = FindTaskNode(TaskName);
                if (Task == null)
                {
                    Print($"Cannot change the name of task {TaskName} because it does not exist!\n");
                    return;
                }

                Task.TaskName = newTaskName;
                TaskNameandIDDic.Add(newTaskName, TaskNameandIDDic[TaskName]);
                TaskNameandIDDic.Remove(TaskName);

                if (Dependencies.ContainsKey(TaskName))
                {
                    Dependencies.Add(newTaskName, Dependencies[TaskName]);
                    Dependencies[newTaskName].TaskName = newTaskName;
                    Dependencies.Remove(TaskName);
                }

                foreach (KeyValuePair<string, DependencyTaskInfo> dependencies in Dependencies)
                {
                    if (dependencies.Value.DependingTasks.ContainsKey(TaskName))
                    {
                        dependencies.Value.DependingTasks.Add(newTaskName, dependencies.Value.DependingTasks[TaskName]);
                        dependencies.Value.DependingTasks.Remove(TaskName);
                    }
                }

                if (graph.vertices.ContainsKey(TaskName)) graph.ChangeVertexName(TaskName, newTaskName);
            }
            else Print($"Cannot change name of task {TaskName} because {TaskName} does not exist or the new name {newTaskName} already exists!\n");
        }

        public void AddTaskToRootTask(string TaskName)
        {
            if (TaskName.Length > 30)
            {
                Print("The name's length is over 30 characters!\n");
                return;
            }
            else if (AlreadyHaveThisTask(TaskName))
            {
                Print($"There already exists a task with this name ({TaskName}).\n");
                Print("Please change the task's name!\n");
                return;
            }

            Task newTask = new Task((++latestLevelOneTaskID).ToString(), TaskName);
            newTask.TaskNodeLevelInTree = 1;
            newTask.ParentTask = RootTask;
            RootTask.SubTasks.Add(latestLevelOneTaskID.ToString(), newTask);

            TaskNameandIDDic.Add(TaskName, latestLevelOneTaskID.ToString());
            TaskIDandSubTaskIDDic.Add(latestLevelOneTaskID.ToString(), new TaskIDandSubtasksID(latestLevelOneTaskID.ToString()));

            TaskIDandSubTaskIDDic["0"].latestPositionID++;
            TaskIDandSubTaskIDDic["0"].SubTaskIDS.Add(latestLevelOneTaskID.ToString());
        }

        public void AddSubtaskToTask(string subTaskName, string TaskName)
        {
            if (subTaskName.Length > 30)
            {
                Print("The name's length is over 30 characters!\n");
                return;
            }
            else if (AlreadyHaveThisTask(subTaskName))
            {
                Print($"There already exists a task with this name ({subTaskName}).\n");
                Print("Please change the task's name!\n");
                return;
            }

            string TaskNameID = GetTaskID(TaskName);
            string subTaskNameID = MakeIDForSubtask(subTaskName, TaskName);

            if (TaskNameID == "" || subTaskNameID == "")
            {
                Print($"Cannot add {subTaskName} to {TaskName}! Please check the name!\n");
                return;
            }

            TaskNameandIDDic.Add(subTaskName, subTaskNameID);
            TaskIDandSubTaskIDDic.Add(subTaskNameID, new TaskIDandSubtasksID(subTaskNameID));

            TaskIDandSubTaskIDDic[TaskNameID].latestPositionID++;
            TaskIDandSubTaskIDDic[TaskNameID].SubTaskIDS.Add(subTaskNameID);

            Task? Task = FindTaskNode(TaskName);

            if (Task == null) Print($"Cannot add {subTaskName} to {TaskName} because {TaskName} does not exist!\n");
            else
            {
                Task subTask = new Task(subTaskNameID, subTaskName);
                subTask.ParentTask = Task;
                subTask.TaskNodeLevelInTree = Task.TaskNodeLevelInTree + 1;

                Task.IsLeafNode = false;
                Task.SubTasks.Add(subTaskNameID, subTask);
            }
        }

        public void DeleteTask(string TaskName)
        {
            string TaskID = GetTaskID(TaskName);
            if (TaskID == "")
            {
                Print($"Cannot delete task {TaskName} because it does not exist!\n");
                return;
            }

            Stack<string> stack = new Stack<string>();
            Stack<string> postOrderTraversal = new Stack<string>();
            stack.Push(TaskID);

            while (stack.Count > 0)
            {
                string DeletedTaskID = stack.Pop();
                postOrderTraversal.Push(DeletedTaskID);

                foreach (string subtaskID in TaskIDandSubTaskIDDic[DeletedTaskID].SubTaskIDS)
                {
                    stack.Push(subtaskID);
                }
            }

            while (postOrderTraversal.Count > 0)
            {
                string DeletedTaskID = postOrderTraversal.Pop();
                string IDName = GetTaskNameFromID(DeletedTaskID);

                if (Dependencies.ContainsKey(IDName))
                {
                    foreach (KeyValuePair<string, TypeLag> depending in Dependencies[IDName].DependingTasks)
                    {
                        DeleteDependency(IDName, depending.Key);
                    }
                }

                if (graph.CheckIfVertexExists(IDName))
                {
                    foreach (KeyValuePair<string, TypeLag> depended in graph.vertices[IDName].Depended_vertices)
                    {
                        DeleteDependency(depended.Key, IDName);
                    }

                    graph.RemoveVertex(IDName);
                }

                Dependencies.Remove(IDName);
                TaskIDandSubTaskIDDic.Remove(DeletedTaskID);
                TaskNameandIDDic.Remove(IDName);
            }

            if (TaskID.Length == 1)
            {
                TaskIDandSubTaskIDDic["0"].SubTaskIDS.Remove(TaskID);
                RootTask.SubTasks.Remove(TaskID);
            }
            else
            {
                string ParentID = TaskID.Substring(0, TaskID.Length - 1);
                TaskIDandSubTaskIDDic[ParentID].SubTaskIDS.Remove(TaskID);

                Task? ParentTask = FindTaskNode(GetTaskNameFromID(ParentID));
                if (ParentTask == null) Print($"Error! ParentTask of {TaskName} cannot be found!\n");
                else ParentTask.SubTasks.Remove(TaskID);
            }
        }

        public List<string> FindLeafTasksOf(string TaskName)
        {
            string TaskID = GetTaskID(TaskName);
            if (TaskID == "")
            {
                Print($"Cannot find leaf tasks of {TaskName} because it does not exist!\n");
                return new List<string>();
            }

            Queue<string> queue = new Queue<string>();
            List<string> leafTaskIDs = new List<string>();
            List<string> leafTasks = new List<string>();

            queue.Enqueue(TaskID);

            while (queue.Count > 0)
            {
                string DeletedTaskID = queue.Dequeue();

                if (TaskIDandSubTaskIDDic[DeletedTaskID].SubTaskIDS.Count == 0) leafTaskIDs.Add(DeletedTaskID);

                foreach (string subtaskID in TaskIDandSubTaskIDDic[DeletedTaskID].SubTaskIDS)
                {
                    queue.Enqueue(subtaskID);
                }
            }

            foreach (string ID in leafTaskIDs)
            {
                leafTasks.Add(GetTaskNameFromID(ID));
            }

            return leafTasks;
        }

        public void AddDependency(string TaskName, string DependingTaskName, string Type)
        {
            Task? Task = FindTaskNode(TaskName);
            Task? DependingTask = FindTaskNode(DependingTaskName);
            if (Task == null || DependingTask == null)
            {
                Print($"Cannot make the dependency because {TaskName} or {DependingTaskName} does not exist!\n");
                return;
            }
            else if (!(Type == "SS" || Type == "FF" || Type == "SF" || Type == "FS"))
            {
                Print("Please choose Dependency Type as follows: SS/FF/FS/SF\n");
                return;
            }
            else if (Dependencies.ContainsKey(TaskName) && Dependencies[TaskName].DependingTasks.ContainsKey(DependingTaskName) && Dependencies[TaskName].DependingTasks[DependingTaskName].Type == Type)
            {
                Print("This dependency already exists!\n");
                return;
            }
            else if (GetTaskID(TaskName).Length != GetTaskID(DependingTaskName).Length)
            {
                string TaskID = GetTaskID(TaskName);
                string DependingTaskID = GetTaskID(DependingTaskName);

                string HigherLevelTaskID = DependingTaskID.Length < TaskID.Length ? DependingTaskID : TaskID;
                string LowerLevelTaskID = HigherLevelTaskID == DependingTaskID ? TaskID : DependingTaskID;

                if (HigherLevelTaskID == LowerLevelTaskID.Substring(0, HigherLevelTaskID.Length))
                {
                    Print($"Cannot make dependency {DependingTaskName} -> {TaskName} because they are in the same hierarchy!\n");
                    return;
                }
            }

            bool AlreadyHasTask = graph.CheckIfVertexExists(TaskName);
            bool AlreadyHasDependingTask = graph.CheckIfVertexExists(DependingTaskName);
            graph.AddEdge(DependingTaskName, TaskName, Type);

            if (graph.CheckingLoop())
            {
                graph.RemoveEdge(DependingTaskName, TaskName);

                if (!AlreadyHasTask) graph.RemoveVertex(TaskName);

                if (!AlreadyHasDependingTask) graph.RemoveVertex(DependingTaskName);

                Print($"Cannot add the dependency because this dependency {DependingTaskName} -> {TaskName} creates loop!\n");
                return;
            }

            if (!Dependencies.ContainsKey(TaskName))
            {
                Dependencies.Add(TaskName, new DependencyTaskInfo(TaskName));
            }

            if (!Dependencies[TaskName].DependingTasks.ContainsKey(DependingTaskName))
            {
                Dependencies[TaskName].DependingTasks.Add(DependingTaskName, new TypeLag(Type));
            }
            else if (Dependencies[TaskName].DependingTasks[DependingTaskName].Type != Type)
            {
                Dependencies[TaskName].DependingTasks[DependingTaskName].Type = Type;
            }

            graph.AddDurationTo(TaskName, Task.Duration);
            graph.AddDurationTo(DependingTaskName, DependingTask.Duration);

            if (AlreadySettingTimeline(TaskName) || AlreadySettingTimeline(DependingTaskName))
            {
                if (AlreadySettingTimeline(TaskName) && AlreadySettingTimeline(DependingTaskName))
                {
                    if (Type == "SS" && DateTime.Compare(Task.StartDate, DependingTask.StartDate) != 0)
                        SetTimeline(TaskName, DependingTask.StartDate, DependingTask.StartDate.AddDays(Task.Duration - 1));
                    else if (Type == "FF" && DateTime.Compare(Task.EndDate, DependingTask.EndDate) != 0)
                        SetTimeline(TaskName, DependingTask.EndDate.AddDays(-Task.Duration + 1), DependingTask.EndDate);
                    else if (Type == "FS")
                    {
                        TimeSpan span = Task.StartDate - DependingTask.EndDate;
                        if (span.Days != 1) SetTimeline(TaskName, DependingTask.EndDate.AddDays(1), DependingTask.EndDate.AddDays(1 + Task.Duration - 1));
                    }
                    else if (Type == "SF" && DateTime.Compare(Task.EndDate, DependingTask.StartDate) != 0)
                        SetTimeline(TaskName, DependingTask.StartDate.AddDays(-Task.Duration + 1), DependingTask.StartDate);
                }
                else if (!AlreadySettingTimeline(TaskName))
                {
                    if (Type == "SS") SetTimeline(TaskName, DependingTask.StartDate, DependingTask.StartDate.AddDays(Task.Duration - 1));
                    else if (Type == "FF") SetTimeline(TaskName, DependingTask.EndDate.AddDays(-Task.Duration + 1), DependingTask.EndDate);
                    else if (Type == "FS") SetTimeline(TaskName, DependingTask.EndDate.AddDays(1), DependingTask.EndDate.AddDays(1 + Task.Duration - 1));
                    else if (Type == "SF") SetTimeline(TaskName, DependingTask.StartDate.AddDays(-Task.Duration + 1), DependingTask.StartDate);
                }
                else if (!AlreadySettingTimeline(DependingTaskName))
                {
                    if (Type == "SS") SetTimeline(DependingTaskName, Task.StartDate, Task.StartDate.AddDays(DependingTask.Duration - 1));
                    else if (Type == "FF") SetTimeline(DependingTaskName, Task.EndDate.AddDays(-DependingTask.Duration + 1), Task.EndDate);
                    else if (Type == "FS") SetTimeline(DependingTaskName, Task.StartDate.AddDays(-1 - DependingTask.Duration + 1), Task.StartDate.AddDays(-1));
                    else if (Type == "SF") SetTimeline(DependingTaskName, Task.EndDate, Task.EndDate.AddDays(DependingTask.Duration - 1));
                }
            }
        }

        public void AddLagToDependency(string TaskName, string DependingTaskName, int Lag)
        {
            Task? Task = FindTaskNode(TaskName);
            Task? DependingTask = FindTaskNode(DependingTaskName);
            if (Task == null || DependingTask == null)
            {
                Print($"Cannot add lag because {TaskName} or {DependingTaskName} does not exist!\n");
                return;
            }
            else if (!Dependencies.ContainsKey(TaskName) || !Dependencies[TaskName].DependingTasks.ContainsKey(DependingTaskName))
            {
                Print($"Cannot add lag to dependency {DependingTaskName} -> {TaskName} because this dependency does not exist!\n");
                return;
            }
            else if (Dependencies[TaskName].DependingTasks[DependingTaskName].Lag == Lag)
            {
                Print($"The lag of dependency {DependingTaskName} -> {TaskName} already = {Lag}\n");
                return;
            }

            string Type = Dependencies[TaskName].DependingTasks[DependingTaskName].Type;
            Dependencies[TaskName].DependingTasks[DependingTaskName].Lag = Lag;
            graph.AddEdge(DependingTaskName, TaskName, Type, Lag);

            if (AlreadySettingTimeline(TaskName) && AlreadySettingTimeline(DependingTaskName))
            {
                TimeSpan span = new TimeSpan();

                if (Type == "SS")
                {
                    span = Task.StartDate - DependingTask.StartDate;
                    if (span.Days != Lag) SetTimeline(TaskName, DependingTask.StartDate.AddDays(Lag), DependingTask.StartDate.AddDays(Lag + Task.Duration - 1), DependingTaskName);
                }
                else if (Type == "FF")
                {
                    span = Task.EndDate - DependingTask.EndDate;
                    if (span.Days != Lag) SetTimeline(TaskName, DependingTask.EndDate.AddDays(Lag - Task.Duration + 1), DependingTask.EndDate.AddDays(Lag), DependingTaskName);
                }
                else if (Type == "FS")
                {
                    span = Task.StartDate - DependingTask.EndDate;
                    if (span.Days - 1 != Lag) SetTimeline(TaskName, DependingTask.EndDate.AddDays(1 + Lag), DependingTask.EndDate.AddDays(1 + Lag + Task.Duration - 1), DependingTaskName);
                }
                else if (Type == "SF")
                {
                    span = Task.EndDate - DependingTask.StartDate;
                    if (span.Days != Lag) SetTimeline(TaskName, DependingTask.StartDate.AddDays(Lag - Task.Duration + 1), DependingTask.StartDate.AddDays(Lag), DependingTaskName);
                }
            }
        }

        public void DeleteDependency(string TaskName, string DependingTaskName)
        {
            Task? Task = FindTaskNode(TaskName);
            Task? DependingTask = FindTaskNode(DependingTaskName);
            if (Task == null || DependingTask == null)
            {
                Print($"Cannot delete this dependency because {TaskName} or {DependingTaskName} does not exist!\n");
                return;
            }
            else if (!Dependencies.ContainsKey(TaskName) || !Dependencies[TaskName].DependingTasks.ContainsKey(DependingTaskName))
            {
                Print($"Cannot delete dependency {DependingTaskName} -> {TaskName} because it does not exist!\n");
                return;
            }

            Dependencies[TaskName].DependingTasks.Remove(DependingTaskName);
            graph.RemoveEdge(DependingTaskName, TaskName);

            if (AlreadySettingTimeline(TaskName) && AlreadySettingTimeline(DependingTaskName))
            {
                List<DateTime> listOfSubtaskStartDates = new List<DateTime>();

                foreach (KeyValuePair<string, TypeLag> depending in Dependencies[TaskName].DependingTasks)
                {
                    DependingTask = FindTaskNode(depending.Key);
                    if (DependingTask == null)
                    {
                        Print($"Error! Cannot find the depending task {DependingTaskName} of {TaskName}!\n");
                        return;
                    }

                    if (depending.Value.Type == "SS") listOfSubtaskStartDates.Add(DependingTask.StartDate.AddDays(depending.Value.Lag));
                    else if (depending.Value.Type == "FF") listOfSubtaskStartDates.Add(DependingTask.EndDate.AddDays(depending.Value.Lag - Task.Duration + 1));
                    else if (depending.Value.Type == "FS") listOfSubtaskStartDates.Add(DependingTask.EndDate.AddDays(1 + depending.Value.Lag));
                    else if (depending.Value.Type == "SF") listOfSubtaskStartDates.Add(DependingTask.StartDate.AddDays(depending.Value.Lag - Task.Duration + 1));
                }

                DateTime LatestStartDate = LatestDateTime(listOfSubtaskStartDates);
                SetTimeline(TaskName, LatestStartDate, LatestStartDate.AddDays(Task.Duration - 1));
            }
        }

        public void SetTimelineForTask(string TaskName, DateTime newStart, DateTime newEnd)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null) Print($"Cannot change the timeline of task {TaskName} because it does not exist!\n");
            else if (!Task.IsLeafNode) Print($"Cannot change the timeline of summary task {TaskName}!\n");
            else if (newStart == DateTime.MinValue || newEnd == DateTime.MinValue) Print($"The timeline of a task must not be set to 1/1/0001!\n");
            else if (newStart > newEnd) Print($"Cannot set timeline because the finish date is before the start date!\n");
            else SetTimeline(TaskName, newStart, newEnd);
        }

        private void SetTimeline(string TaskName, DateTime newStart, DateTime newEnd, string excluded = "")
        {
            Task? Task = FindTaskNode(TaskName);
            TimeSpan span = newEnd - newStart;
            if (Task == null)
            {
                Print($"Cannot set timeline of task {TaskName} because it does not exist!\n");
                return;
            }

            if (Dependencies.ContainsKey(TaskName))
            {
                foreach (KeyValuePair<string, TypeLag> depending in Dependencies[TaskName].DependingTasks)
                {
                    if (depending.Key != excluded && CreateConflict(TaskName, depending.Key, depending.Value.Type, newStart, newEnd) && AlreadySettingTimeline(depending.Key))
                    {
                        Task? dependingTask = FindTaskNode(depending.Key);
                        if (dependingTask == null)
                        {
                            Print($"Error! Depending task {depending.Key} of {TaskName} does not exist!\n");
                            return;
                        }

                        Print($"Conflict occurs with dependency {depending.Key} -> {TaskName} type {depending.Value.Type}{depending.Value.Lag}!\n");
                        Print($"-Depending task: {depending.Key}, ID = {dependingTask.TaskID}:\n");
                        Print($" +Start Date = {dependingTask.StartDate.ToString("MM/dd/yyyy")}\n");
                        Print($" +End Date = {dependingTask.EndDate.ToString("MM/dd/yyyy")}\n");
                        Print($"-Depended task: {TaskName}, ID = {Task.TaskID}:\n");
                        Print($" +New Start Date = {newStart.ToString("MM/dd/yyyy")}\n");
                        Print($" +New End Date = {newEnd.ToString("MM/dd/yyyy")}\n");
                        Print($" +Current Start Date = {Task.StartDate.ToString("MM/dd/yyyy")}\n");
                        Print($" +Current End Date = {Task.EndDate.ToString("MM/dd/yyyy")}\n\n");
                        return;
                    }
                }

                foreach (KeyValuePair<string, TypeLag> depending in Dependencies[TaskName].DependingTasks)
                {
                    if (!AlreadySettingTimeline(depending.Key))
                    {
                        Task? dependingTask = FindTaskNode(depending.Key);
                        if (dependingTask == null)
                        {
                            Print($"Cannot change timeline because depending task {depending.Key} does not exist!\n");
                            return;
                        }

                        int Lag = Dependencies[TaskName].DependingTasks[depending.Key].Lag;
                        string Type = depending.Value.Type;

                        if (Type == "SS") SetTimeline(depending.Key, newStart.AddDays(-Lag), newStart.AddDays(-Lag + dependingTask.Duration - 1));
                        else if (Type == "FF") SetTimeline(depending.Key, newEnd.AddDays(-Lag - dependingTask.Duration + 1), newEnd.AddDays(-Lag));
                        else if (Type == "FS") SetTimeline(depending.Key, newStart.AddDays(-1 - Lag - dependingTask.Duration + 1), newStart.AddDays(-1 - Lag));
                        else if (Type == "SF") SetTimeline(depending.Key, newEnd.AddDays(-Lag), newEnd.AddDays(-Lag + dependingTask.Duration - 1));
                    }
                }
            }

            Task.StartDate = newStart;
            Task.EndDate = newEnd;
            Task.Duration = span.Days + 1;

            if (graph.CheckIfVertexExists(TaskName))
            {
                graph.AddDurationTo(TaskName, Task.Duration);

                foreach (KeyValuePair<string, TypeLag> depended in graph.vertices[TaskName].Depended_vertices)
                {
                    Task? dependedTask = FindTaskNode(depended.Key);
                    if (dependedTask == null)
                    {
                        Print($"Cannot set timeline because the depended task {depended.Key} cannot be found!\n");
                        return;
                    }

                    int Lag = depended.Value.Lag;
                    string Type = depended.Value.Type;
                    TimeSpan newSpan = new TimeSpan();

                    if (Type == "SS")
                    {
                        newSpan = dependedTask.StartDate - Task.StartDate;
                        if (!AlreadySettingTimeline(depended.Key) || (newSpan.Days != Lag && Lag != 0))
                            SetTimeline(depended.Key, newStart.AddDays(Lag), newStart.AddDays(Lag + dependedTask.Duration - 1), TaskName);
                        else if (Lag == 0) SetTimeline(depended.Key, newStart, newStart.AddDays(dependedTask.Duration - 1));
                    }
                    else if (Type == "FF")
                    {
                        newSpan = dependedTask.EndDate - Task.EndDate;
                        if (!AlreadySettingTimeline(depended.Key) || (newSpan.Days != Lag && Lag != 0))
                            SetTimeline(depended.Key, newEnd.AddDays(Lag - dependedTask.Duration + 1), newEnd.AddDays(Lag), TaskName);
                        else if (Lag == 0) SetTimeline(depended.Key, newEnd.AddDays(-dependedTask.Duration + 1), newEnd);
                    }
                    else if (Type == "FS")
                    {
                        newSpan = dependedTask.StartDate - Task.EndDate;
                        if (!AlreadySettingTimeline(depended.Key) || (newSpan.Days - 1 != Lag && Lag != 0))
                            SetTimeline(depended.Key, newEnd.AddDays(1 + Lag), newEnd.AddDays(1 + Lag + dependedTask.Duration - 1), TaskName);
                        else if (Lag == 0) SetTimeline(depended.Key, newEnd.AddDays(1), newEnd.AddDays(1 + dependedTask.Duration - 1));
                    }
                    else if (Type == "SF")
                    {
                        newSpan = dependedTask.EndDate - Task.StartDate;
                        if (!AlreadySettingTimeline(depended.Key) || (newSpan.Days != Lag && Lag != 0))
                            SetTimeline(depended.Key, newStart.AddDays(Lag - dependedTask.Duration + 1), newStart.AddDays(Lag), TaskName);
                        else if (Lag == 0) SetTimeline(depended.Key, newStart.AddDays(-dependedTask.Duration + 1), newStart);
                    }
                }
            }

            if (Task.ParentTask != null)
            {
                List<DateTime> listOfSubtaskStartDates = new List<DateTime>();
                List<DateTime> listOfSubtaskEndDates = new List<DateTime>();

                foreach (KeyValuePair<string, Task> subTask in Task.ParentTask.SubTasks)
                {
                    if (AlreadySettingTimeline(subTask.Value.TaskName))
                    {
                        listOfSubtaskStartDates.Add(subTask.Value.StartDate);
                        listOfSubtaskEndDates.Add(subTask.Value.EndDate);
                    }
                }

                DateTime EarliestDate = EarliestDateTime(listOfSubtaskStartDates);
                DateTime LatestDate = LatestDateTime(listOfSubtaskEndDates);
                SetTimeline(Task.ParentTask.TaskName, EarliestDate, LatestDate);
            }
        }

        public void ChangeDuration(string TaskName, int newDuration)
        {
            Task? Task = FindTaskNode(TaskName);
            if (newDuration < 1)
            {
                Print($"Duration of a task must be greater than 1!\n");
                return;
            }
            else if (Task == null)
            {
                Print($"Cannot change duration of task {TaskName} because it does not exist!\n");
                return;
            }
            else if (!Task.IsLeafNode)
            {
                Print($"Cannot change duration because {TaskName} is a summary task!\n");
                return;
            }

            if (!AlreadySettingTimeline(TaskName))
            {
                Task.Duration = newDuration;
                return;
            }

            int difference = newDuration - Task.Duration;
            DateTime oldStartDate = Task.StartDate;
            SetTimeline(TaskName, Task.StartDate.AddDays(-difference), Task.EndDate);

            if (Task.StartDate != oldStartDate.AddDays(-difference))
            {
                DateTime oldEndDate = Task.EndDate;
                SetTimeline(TaskName, Task.StartDate, Task.EndDate.AddDays(difference));

                if (Task.EndDate != oldEndDate.AddDays(difference))
                {
                    Print($"Fail to change duration from {Task.Duration} to {newDuration} of task {TaskName}!\n");
                }
            }
        }

        public List<string> EarliestTasks(List<string> Tasks)
        {
            if (Tasks.Count == 0)
            {
                Print($"The list is empty!\n");
                Print($"Warning! Return empty list of strings.\n");
                return new List<string>();
            }

            List<string> earliestTasks = new List<string>();
            List<Task> TaskNodes = new List<Task>();

            foreach (string task in Tasks)
            {
                Task? TaskNode = FindTaskNode(task);
                if (TaskNode != null) TaskNodes.Add(TaskNode);
            }

            List<DateTime> listOfSubtaskStartDates = new List<DateTime>();

            foreach (Task TaskNode in TaskNodes)
            {
                if (AlreadySettingTimeline(TaskNode.TaskName))
                {
                    listOfSubtaskStartDates.Add(TaskNode.StartDate);
                }
            }

            DateTime EarliestDate = EarliestDateTime(listOfSubtaskStartDates);

            foreach (Task TaskNode in TaskNodes)
            {
                if (TaskNode.StartDate == EarliestDate)
                {
                    earliestTasks.Add(TaskNode.TaskName);
                }
            }

            if (earliestTasks.Count == 0) Print($"Warning! Return empty list of strings.\n");
            return earliestTasks;
        }

        public List<string> LatestTasks(List<string> Tasks)
        {
            if (Tasks.Count == 0)
            {
                Print($"The list is empty!\n");
                Print($"Warning! Return empty list of strings.\n");
                return new List<string>();
            }

            List<string> latestTasks = new List<string>();
            List<Task> TaskNodes = new List<Task>();

            foreach (string task in Tasks)
            {
                Task? TaskNode = FindTaskNode(task);
                if (TaskNode != null) TaskNodes.Add(TaskNode);
            }

            List<DateTime> listOfSubtaskEndDates = new List<DateTime>();

            foreach (Task TaskNode in TaskNodes)
            {
                if (AlreadySettingTimeline(TaskNode.TaskName))
                {
                    listOfSubtaskEndDates.Add(TaskNode.EndDate);
                }
            }

            DateTime LatestDate = LatestDateTime(listOfSubtaskEndDates);

            foreach (Task TaskNode in TaskNodes)
            {
                if (TaskNode.EndDate == LatestDate)
                {
                    latestTasks.Add(TaskNode.TaskName);
                }
            }

            if (latestTasks.Count == 0) Print($"Warning! Return empty list of strings.\n");
            return latestTasks;
        }

        private DateTime EarliestDateTime(List<DateTime> DateTimes)
        {
            DateTime min = DateTime.MaxValue;

            foreach (DateTime dateTime in DateTimes)
            {
                if (DateTime.Compare(min, dateTime) > 0)
                {
                    min = dateTime;
                }
            }

            if (DateTimes.Count == 0)
            {
                Print($"Warning! DateTimes list's length = 0!\n");
                Print($"Return {min}\n");
            }

            return min;
        }

        private DateTime LatestDateTime(List<DateTime> DateTimes)
        {
            DateTime max = DateTime.MinValue;

            foreach (DateTime dateTime in DateTimes)
            {
                if (DateTime.Compare(max, dateTime) < 0)
                {
                    max = dateTime;
                }
            }

            if (DateTimes.Count == 0)
            {
                Print($"Warning! DateTimes list's length = 0!\n");
                Print($"Return {max}\n");
            }

            return max;
        }

        private bool CreateConflict(string TaskName, string DependingTaskName, string Type, DateTime newStart, DateTime newEnd)
        {
            Task? DependingTask = FindTaskNode(DependingTaskName);
            if (DependingTask == null)
            {
                Print($"Depending task {DependingTaskName} of task {TaskName} cannot be found!\n");
                return false;
            }

            int Lag = Dependencies[TaskName].DependingTasks[DependingTaskName].Lag;
            TimeSpan newSpan = new TimeSpan();

            if (Lag == 0)
            {
                if (Type == "SS" && DateTime.Compare(newStart, DependingTask.StartDate) < 0) return true;
                else if (Type == "FF" && DateTime.Compare(newEnd, DependingTask.EndDate) < 0) return true;
                else if (Type == "FS" && DateTime.Compare(newStart, DependingTask.EndDate) <= 0) return true;
                else if (Type == "SF" && DateTime.Compare(newEnd, DependingTask.StartDate) < 0) return true;
                else return false;
            }
            else
            {
                if (Type == "SS")
                {
                    newSpan = newStart - DependingTask.StartDate;
                    if (newSpan.Days == Lag || DateTime.Compare(newStart, DependingTask.StartDate) >= 0) return false;
                    else return true;
                }
                else if (Type == "FF")
                {
                    newSpan = newEnd - DependingTask.EndDate;
                    if (newSpan.Days == Lag || DateTime.Compare(newEnd, DependingTask.EndDate) >= 0) return false;
                    else return true;
                }
                else if (Type == "FS")
                {
                    newSpan = newStart - DependingTask.EndDate;
                    if (newSpan.Days - 1 == Lag || DateTime.Compare(newStart, DependingTask.EndDate) > 0) return false;
                    else return true;
                }
                else if (Type == "SF")
                {
                    newSpan = newEnd - DependingTask.StartDate;
                    if (newSpan.Days == Lag || DateTime.Compare(newEnd, DependingTask.StartDate) >= 0) return false;
                    else return true;
                }
                else return false;
            }
        }

        public void UpdateStatus(string TaskName, string Status)
        {
            Task? Task = FindTaskNode(TaskName);
            if (!(Status == "Not start" || Status == "In progress" || Status == "Complete"))
            {
                Print("PLease choose status as follows: Not start/In progress/Complete\n");
                return;
            }
            else if (Task == null)
            {
                Print($"Cannot update the status of task {TaskName} because it does not exist!\n");
                return;
            }
            else if (!Task.IsLeafNode)
            {
                Print($"Cannot change the status of task {TaskName} because it is a summary task!\n");
                return;
            }

            if (Status == "In progress" && Dependencies.ContainsKey(TaskName)) 
            {
                foreach (KeyValuePair<string, TypeLag> depending in Dependencies[TaskName].DependingTasks)
                {
                    if (depending.Value.Type == "SS" || depending.Value.Type == "FS")
                    {
                        Task? DependingTask = FindTaskNode(depending.Key);
                        if (DependingTask == null)
                        {
                            Print($"Cannot found the depending task {depending.Key} of {TaskName} because it does not exist!\n");
                            return;
                        }

                        int Lag = depending.Value.Lag;
                        string Type = depending.Value.Type;

                        if (Lag >= 0)
                        {
                            if(Type == "SS" && DependingTask.Status == "Not start")
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag);
                                return;
                            }
                            else if(Type == "FS" && DependingTask.Status != "Complete")
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag);
                                return;
                            }
                        }
                        else
                        {
                            if (Type == "SS" && !(CurrentDate >= DependingTask.StartDate.AddDays(Lag) && CurrentDate >= Task.StartDate))
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag, true, "Start", "Start");
                                return;
                            }
                            else if (Type == "FS" && !(DependingTask.Status == "In progress" && CurrentDate >= DependingTask.EndDate.AddDays(1 + Lag) && CurrentDate >= Task.StartDate))
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag, true, "End", "Start");
                                return;
                            }
                        }
                    }
                }
            }
            else if (Status == "Complete" && Dependencies.ContainsKey(TaskName))
            {
                foreach (KeyValuePair<string, TypeLag> depending in Dependencies[TaskName].DependingTasks)
                {
                    if (depending.Value.Type == "FF" || depending.Value.Type == "SF")
                    {
                        Task? DependingTask = FindTaskNode(depending.Key);
                        if (DependingTask == null)
                        {
                            Print($"Cannot found the depending task {depending.Key} of {TaskName} because it does not exist!\n");
                            return;
                        }

                        int Lag = depending.Value.Lag;
                        string Type = depending.Value.Type;

                        if (Lag >= 0)
                        {
                            if((Type == "SS" || Type == "SF") && DependingTask.Status == "Not start")
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag);
                                return;
                            }
                            else if((Type == "FF" || Type == "FS") && DependingTask.Status != "Complete")
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag);
                                return;
                            }
                        }
                        else
                        {
                            if (Type == "SF" && !(CurrentDate >= DependingTask.StartDate.AddDays(Lag) && CurrentDate >= Task.EndDate))
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag, true, "Start", "End");
                                return;
                            }
                            else if (Type == "SS" && !(CurrentDate >= DependingTask.StartDate.AddDays(Lag) && CurrentDate >= Task.StartDate))
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag, true, "Start", "Start");
                                return;
                            }
                            else if (Type == "FF" && !(DependingTask.Status == "In progress" && CurrentDate >= DependingTask.EndDate.AddDays(Lag) && CurrentDate >= Task.EndDate))
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag, true, "End", "End");
                                return;
                            }
                            else if (Type == "FS" && !(DependingTask.Status == "In progress" && CurrentDate >= DependingTask.EndDate.AddDays(1 + Lag) && CurrentDate >= Task.StartDate))
                            {
                                PrintStatusError(TaskName, depending.Key, Status, Type, Lag, true, "End", "Start");
                                return;
                            }
                        }
                    }
                }
            }

            Task.Status = Status;
            UpdateStatusToParentTaskOfTask(TaskName);
            if (Status == "Complete") UpdatePercentageComplete(TaskName, 100);
        }

        private void PrintStatusError(string TaskName, string DependingTaskName, string Status, string Type, int Lag, bool PrintCurrentDate = false, string DependingTaskDate = "", string TaskDate = "")
        {
            Task? Task = FindTaskNode(TaskName);
            Task? DependingTask = FindTaskNode(DependingTaskName);
            if (Task == null || DependingTask == null) return;

            Print($"Cannot set the status of {TaskName} to {Status} because of dependency {DependingTaskName} -> {TaskName} type {Type}{Lag}!\n");
            Print($"Depending task {DependingTaskName} status: {DependingTask.Status}\n");

            if(PrintCurrentDate) Print($"Current Date: {CurrentDate.ToString("MM/dd/yyyy")}\n");

            if(DependingTaskDate == "Start")
            {
                Print($"DependingTask {DependingTaskName} StartDate: {DependingTask.StartDate.ToString("MM/dd/yyyy")}\n");
                Print($"DependingTask {DependingTaskName} StartDate with Lag {Lag}: {DependingTask.StartDate.AddDays(Lag).ToString("MM/dd/yyyy")}\n");
            }
            else if(DependingTaskDate == "End")
            {
                Print($"DependingTask {DependingTaskName} EndDate: {DependingTask.EndDate.ToString("MM/dd/yyyy")}\n");
                if(Type == "FF") Print($"DependingTask {DependingTaskName} EndDate with Lag {Lag}: {DependingTask.EndDate.AddDays(Lag).ToString("MM/dd/yyyy")}\n");
                else if(Type == "FS") Print($"DependingTask {DependingTaskName} EndDate with Lag {Lag}: {DependingTask.EndDate.AddDays(1 + Lag).ToString("MM/dd/yyyy")}\n");
            }

            if(TaskDate == "Start") Print($"Task {TaskName} StartDate: {Task.StartDate.ToString("MM/dd/yyyy")}\n");
            else if (TaskDate == "End") Print($"Task {TaskName} EndDate: {Task.EndDate.ToString("MM/dd/yyyy")}\n");
        }

        private void UpdateStatusToParentTaskOfTask(string TaskName)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null || Task.ParentTask == null) return;

            if (Task.Status == "Not start" && Task.ParentTask.Status != "Not start")
            {
                bool statusChangeable = true;

                foreach (KeyValuePair<string, Task> subtask in Task.ParentTask.SubTasks)
                {
                    if (subtask.Value.Status != "Not start")
                    {
                        statusChangeable = false;
                        break;
                    }
                }

                if (statusChangeable)
                {
                    Task.ParentTask.Status = "Not start";
                    UpdateStatusToParentTaskOfTask(Task.ParentTask.TaskName);
                }
            }
            else if (Task.Status == "In progress" && Task.ParentTask.Status != "In progress")
            {
                Task.ParentTask.Status = "In progress";
                UpdateStatusToParentTaskOfTask(Task.ParentTask.TaskName);
            }
            else if (Task.Status == "Complete" && Task.ParentTask.Status != "Complete")
            {
                bool statusChangeable = true;

                foreach (KeyValuePair<string, Task> subtask in Task.ParentTask.SubTasks)
                {
                    if (subtask.Value.Status != "Complete")
                    {
                        statusChangeable = false;
                        break;
                    }
                }

                if (statusChangeable)
                {
                    Task.ParentTask.Status = "Complete";
                    UpdateStatusToParentTaskOfTask(Task.ParentTask.TaskName);
                }
                else if (Task.ParentTask.Status == "Not start")
                {
                    Task.ParentTask.Status = "In progress";
                    UpdateStatusToParentTaskOfTask(Task.ParentTask.TaskName);
                }
            }
        }

        private void UpdatePercentageComplete(string TaskName, int Percentage)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot update task {TaskName} because it does not exist!\n");
                return;
            }

            Task.PercentageCompleted = Percentage;
            if (Task.ParentTask == null) return;
            int sum = 0;

            if (Task.ParentTask.Status == "Complete") UpdatePercentageComplete(Task.ParentTask.TaskName, 100);
            else
            {
                foreach (KeyValuePair<string, Task> subtask in Task.ParentTask.SubTasks)
                {
                    sum += subtask.Value.PercentageCompleted;
                }

                if (Task.ParentTask.SubTasks.Count == 0) Print($"Error! Parent Task {Task.ParentTask.TaskName} of {TaskName} has no Subtasks!\n");
                else UpdatePercentageComplete(Task.ParentTask.TaskName, sum / Task.ParentTask.SubTasks.Count);
            }
        }

        public void AddDescriptionToTask(string TaskName, string Description)
        {
            Task? task = FindTaskNode(TaskName);
            if (task == null) Print($"Cannot add description to task {TaskName} because it does not exist!\n");
            else task.Desription.Add(Description);
        }

        public void DeleteDescriptionOfTaskAtLine(string TaskName, int Line)
        {
            Task? task = FindTaskNode(TaskName);
            if (task == null) Print($"Cannot delete description from task {TaskName} because it does not exist!\n");
            else
            {
                if (Line > task.Desription.Count || Line <= 0) Print($"Cannot delete description at line {Line}!\n");
                else task.Desription.RemoveAt(Line - 1);
            }
        }

        public void DeleteAllDescriptionOfTask(string TaskName)
        {
            Task? task = FindTaskNode(TaskName);
            if (task == null) Print($"Cannot delete all description from task {TaskName} because it does not exist!\n");
            else task.Desription.Clear();
        }

        public void SetPriorityOfTask(string TaskName, string Priority)
        {
            if (Priority == "" || Priority == "Low" || Priority == "Medium" || Priority == "High")
            {
                Task? task = FindTaskNode(TaskName);
                if (task == null) Print($"Cannot set priority to task {TaskName} because it does not exist!\n");
                else task.Priority = Priority;
            }
            else Print("Please set priority in these categories: Low/Medium/High or put empty\n");
        }

        public void SetWorkingHoursPerDayFOfTask(string TaskName, float hours)
        {
            if (hours < 0f || hours > 24) Print("The value for working hours per day must not be less than zero or greater than 24!\n");
            else
            {
                Task? task = FindTaskNode(TaskName);
                if (task == null) Print($"Cannot set working hours per day because task {TaskName} does not exist!\n");
                else task.TaskWorkingHoursPerDay = hours;
            }
        }

        public void AddResourceToTask(string ResourceName, string TaskName)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null) return;
            else if (Task.ResourceAndCapacityDic.ContainsKey(ResourceName)) return;
            else Task.ResourceAndCapacityDic.Add(ResourceName, 1);
        }

        public void AddCapacityToResourceOfTask(string ResourceName, string TaskName, int Capacity)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null) return;
            else if (!Task.ResourceAndCapacityDic.ContainsKey(ResourceName)) return;
            else if (Capacity < 1) return;
            else Task.ResourceAndCapacityDic[ResourceName] = Capacity;
        }

        public void DeleteResourceOfTask(string ResourceName, string TaskName)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null) return;
            else if (!Task.ResourceAndCapacityDic.ContainsKey(ResourceName)) return;
            else Task.ResourceAndCapacityDic.Remove(ResourceName);
        }

        public void DeleteAllResourceOfTask(string TaskName)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null) return;
            else Task.ResourceAndCapacityDic.Clear();
        }

        public void PrintDetailedResourcesAndTotalCostInfoOfTask(string TaskName, ResourceManagement Resources)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot print resource and cost information of task {TaskName} because it does not exist!\n");
                return;
            }

            if (!Task.IsLeafNode) PrintResourceBarTitle(TaskName, "Summary task", CalculateCostOfSummaryTask(TaskName, Resources), Resources.Currency);
            else PrintResourceBarTitle(TaskName, "Subtask", CalculateCostOfSubtask(TaskName, Resources), Resources.Currency);
            Print("\n");

            foreach (KeyValuePair<string, int> resource in Task.ResourceAndCapacityDic)
            {
                Print("|");
                PrintWordWithEmptySpace(resource.Key, 21);
                PrintWordWithEmptySpace(resource.Value.ToString(), 11);

                if (Resources.CheckIfWorkResourceExists(resource.Key))
                {
                    PrintWordWithEmptySpace("Work", 10);

                    if (Resources.WorkResourceList[resource.Key].Accrue == "Start")
                    {
                        float WorkResourceCost = CalculateCostStartAccrueOfWorkResource(resource.Key, TaskName, Resources);
                        PrintWordWithEmptySpace("Start", 10);

                        if (CurrentDate >= Task.StartDate) PrintWordWithEmptySpace($"{WorkResourceCost}{Resources.Currency}", 11);
                        else PrintWordWithEmptySpace(" ", 11);
                    }
                    else if(Resources.WorkResourceList[resource.Key].Accrue == "End")
                    {
                        float WorkResourceCost = CalculateCostEndAccrueOfWorkResource(resource.Key, TaskName, Resources);
                        PrintWordWithEmptySpace("End", 10);

                        if (CurrentDate >= Task.EndDate) PrintWordWithEmptySpace($"{WorkResourceCost}{Resources.Currency}", 11);
                        else PrintWordWithEmptySpace(" ", 11);
                    }
                    else if(Resources.WorkResourceList[resource.Key].Accrue == "Prorated")
                    {
                        float WorkResourceCost = CalculateCostProratedAccrueOfWorkResource(resource.Key, TaskName, Resources);
                        PrintWordWithEmptySpace("Prorated", 10);

                        if (CurrentDate >= Task.StartDate) PrintWordWithEmptySpace($"{WorkResourceCost}{Resources.Currency}", 11);
                        else PrintWordWithEmptySpace(" ", 11);
                    }
                }
                else if (Resources.CheckIfMaterialResourceExists(resource.Key))
                {
                    PrintWordWithEmptySpace("Material", 10);
                    PrintWordWithEmptySpace(" ", 10);
                    PrintWordWithEmptySpace(CalculateTotalCostOfaResourceInTask(resource.Key, TaskName, Resources).ToString(), 11);
                }
                
                Print("\n");
            }
        }

        private float CalculateCostOfSubtask(string TaskName, ResourceManagement Resources)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot calculate cost because {TaskName} does not exist!\n");
                return 0;
            }

            float cost = 0;

            foreach (KeyValuePair<string, int> resource in Task.ResourceAndCapacityDic)
            {
                if (Resources.CheckIfWorkResourceExists(resource.Key))
                {
                    if (Resources.WorkResourceList[resource.Key].Accrue == "Start")
                        cost += CalculateCostStartAccrueOfWorkResource(resource.Key, TaskName, Resources);
                    else if (Resources.WorkResourceList[resource.Key].Accrue == "End")
                        cost += CalculateCostEndAccrueOfWorkResource(resource.Key, TaskName, Resources);
                    else if (Resources.WorkResourceList[resource.Key].Accrue == "Prorated")
                        cost += CalculateCostProratedAccrueOfWorkResource(resource.Key, TaskName, Resources);
                }
                else if (Resources.CheckIfMaterialResourceExists(resource.Key)) cost += CalculateTotalCostOfaResourceInTask(resource.Key, TaskName, Resources);
            }

            return cost;
        }

        private float CalculateCostProratedAccrueOfWorkResource(string ResourceName, string TaskName, ResourceManagement Resources)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot calculate cost because {TaskName} does not exist!\n");
                return 0;
            }
            else if (!Task.ResourceAndCapacityDic.ContainsKey(ResourceName))
            {
                Print($"Resource {ResourceName} does not exist in task {TaskName}!\n");
                return 0;
            }
            else if (!Resources.CheckIfWorkResourceExists(ResourceName)) Print($"Resource {ResourceName} does not exist!\n");

            float cost = 0;

            if (CurrentDate >= Task.StartDate && CurrentDate <= Task.EndDate)
            {
                float WorkingHoursPerDayOfTask = Task.TaskWorkingHoursPerDay;
                float MaximumWorkingHoursOfWorkResource = Resources.WorkResourceList[ResourceName].MaximumWorkingHoursPerDay;
                float StandardRate = Resources.WorkResourceList[ResourceName].StandardRate;
                float OvertimeRate = Resources.WorkResourceList[ResourceName].OvertimeRate;
                int Capacity = Task.ResourceAndCapacityDic[ResourceName];

                float costPerDay = WorkingHoursPerDayOfTask <= MaximumWorkingHoursOfWorkResource
                                                     ? WorkingHoursPerDayOfTask * StandardRate
                                                     : MaximumWorkingHoursOfWorkResource * StandardRate + (WorkingHoursPerDayOfTask - MaximumWorkingHoursOfWorkResource) * OvertimeRate;

                TimeSpan span = CurrentDate - Task.StartDate;
                cost += costPerDay * (span.Days + 1) * Capacity;
            }
            else if (CurrentDate > Task.EndDate) cost += CalculateTotalCostOfaResourceInTask(ResourceName, TaskName, Resources);

            return cost;
        }

        private float CalculateCostStartAccrueOfWorkResource(string ResourceName, string TaskName, ResourceManagement Resources)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot calculate cost because {TaskName} does not exist!\n");
                return 0;
            }
            else if (!Task.ResourceAndCapacityDic.ContainsKey(ResourceName))
            {
                Print($"Resource {ResourceName} does not exist in task {TaskName}!\n");
                return 0;
            }

            float cost = 0;

            if (CurrentDate >= Task.StartDate)
            {
                cost += CalculateTotalCostOfaResourceInTask(ResourceName, TaskName, Resources);
            }

            return cost;
        }

        private float CalculateCostEndAccrueOfWorkResource(string ResourceName, string TaskName, ResourceManagement Resources) 
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot calculate cost because {TaskName} does not exist!\n");
                return 0;
            }
            else if (!Task.ResourceAndCapacityDic.ContainsKey(ResourceName))
            {
                Print($"Resource {ResourceName} does not exist in task {TaskName}!\n");
                return 0;
            }

            float cost = 0;

            if (CurrentDate >= Task.EndDate)
            {
                cost += CalculateTotalCostOfaResourceInTask(ResourceName, TaskName, Resources);
            }

            return cost;
        }

        private float CalculateCostOfSummaryTask(string TaskName, ResourceManagement Resources)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot calculate cost because {TaskName} does not exist!\n");
                return 0;
            }

            float TotalCost = 0;

            foreach (KeyValuePair<string, int> resource in Task.ResourceAndCapacityDic)
            {
                TotalCost += CalculateTotalCostOfaResourceInTask(resource.Key, TaskName, Resources);
            }

            foreach (KeyValuePair<string, Task> subtask in Task.SubTasks)
            {
                TotalCost += CalculateCostOfSummaryTask(subtask.Value.TaskName, Resources);
            }

            return TotalCost;
        }

        private float CalculateTotalCostOfaResourceInTask(string ResourceName, string TaskName, ResourceManagement Resources) 
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot calculate cost because {TaskName} does not exist!\n");
                return 0;
            }
            else if (!Task.ResourceAndCapacityDic.ContainsKey(ResourceName))
            {
                Print($"Resource {ResourceName} does not exist in task {TaskName}!\n");
                return 0;
            }

            float cost = 0; 

            if (Resources.CheckIfWorkResourceExists(ResourceName))
            {
                float WorkingHoursPerDayOfTask = Task.TaskWorkingHoursPerDay;
                float MaximumWorkingHoursOfWorkResource = Resources.WorkResourceList[ResourceName].MaximumWorkingHoursPerDay;
                float StandardRate = Resources.WorkResourceList[ResourceName].StandardRate;
                float OvertimeRate = Resources.WorkResourceList[ResourceName].OvertimeRate;
                int Capacity = Task.ResourceAndCapacityDic[ResourceName];

                float costPerDay = WorkingHoursPerDayOfTask <= MaximumWorkingHoursOfWorkResource
                                                 ? WorkingHoursPerDayOfTask * StandardRate
                                                 : MaximumWorkingHoursOfWorkResource * StandardRate + (WorkingHoursPerDayOfTask - MaximumWorkingHoursOfWorkResource) * OvertimeRate;

                cost += costPerDay * Task.Duration * Capacity;
            }
            else if (Resources.CheckIfMaterialResourceExists(ResourceName))
                cost += Resources.MaterialResourceList[ResourceName].StandardRate * Task.ResourceAndCapacityDic[ResourceName];
            else Print($"Resource {ResourceName} does not exist!\n");

            return cost;
        }

        public Task? FindTaskNode(string TaskName)
        {
            if (!AlreadyHaveThisTask(TaskName))
            {
                Print($"Cannot find \"{TaskName}\" because it does not exist!\n");
                return null;
            }
            else
            {
                string TaskNameID = GetTaskID(TaskName);
                int lengthOfID = 1;
                if (TaskNameID == "0") return RootTask;

                Task x = RootTask.SubTasks[TaskNameID.Substring(0, lengthOfID)];

                while (x.TaskID != TaskNameID)
                {
                    x = x.SubTasks[TaskNameID.Substring(0, ++lengthOfID)];
                }

                return x;
            }
        }

        private string MakeIDForSubtask(string subTaskName, string TaskName)
        {
            if (!AlreadyHaveThisTask(TaskName))
            {
                Print($"Cannot make ID for task \"{TaskName}\" because it does not exist!\n");
                return "";
            }
            else if (AlreadyHaveThisTask(subTaskName))
            {
                Print("There already exists a task with this name.\n");
                Print("Please change the task's name!\n");
                return "";
            }
            else
            {
                string TaskNameID = GetTaskID(TaskName);
                int suffixNumberID = TaskIDandSubTaskIDDic[TaskNameID].latestPositionID + 1;
                string subTaskID = TaskNameID + suffixNumberID;
                return subTaskID;
            }
        }

        public bool AlreadySettingTimeline(string TaskName)
        {
            Task? task = FindTaskNode(TaskName);
            if (task == null)
            {
                Print($"Cannot check the timeline of {TaskName} because it does not exist!\n");
                return false;
            }
            else if (task.StartDate == DateTime.MinValue || task.EndDate == DateTime.MinValue) return false;
            else return true;
        }

        public bool AlreadyHaveThisTask(string TaskName)
        {
            if (TaskNameandIDDic.ContainsKey(TaskName)) return true;
            else return false;
        }

        public string GetTaskNameFromID(string TaskID)
        {
            string TaskName = "";

            foreach (KeyValuePair<string, string> name in TaskNameandIDDic)
            {
                if (name.Value == TaskID)
                {
                    TaskName = name.Key;
                    break;
                }
            }

            if (TaskName == "")
            {
                Print($"Cannot find the task's name of ID {TaskID} because this ID does not exist!\n");
            }

            return TaskName;
        }

        public string GetTaskID(string TaskName)
        {
            if (AlreadyHaveThisTask(TaskName)) return TaskNameandIDDic[TaskName];
            else
            {
                Print($"Cannot find ID from task {TaskName} because {TaskName} does not exist!\n");
                return "";
            }
        }

        public void PrintAllTasksInfo()
        {
            Print("____________________________________________________________________________________________________________________________________________________\n");
            PrintInfoBarTitle(); Print("\n");
            PrintAllTasksInfomation();
        }

        private void PrintAllTasksInfomation(string TaskID = "0")
        {
            PrintFullInfoFromID(TaskID); Print("\n");

            foreach (string SubtaskID in TaskIDandSubTaskIDDic[TaskID].SubTaskIDS)
            {
                PrintAllTasksInfomation(SubtaskID);
            }
        }

        public void PrintInfoOfTask(string TaskName)
        {
            Print("____________________________________________________________________________________________________________________________________________________\n");
            PrintInfoBarTitle(); Print("\n");
            PrintFullInfoFromID(GetTaskID(TaskName)); Print("\n");
        }

        private void PrintFullInfoFromID(string TaskID)
        {
            Task? Task = FindTaskNode(GetTaskNameFromID(TaskID));
            if (Task == null)
            {
                Print($"Cannot find information from task with ID {TaskID}\n");
                return;
            }

            Print("|");
            PrintWordWithEmptySpace(Task.TaskID, 10);
            PrintWordWithEmptySpace(Task.TaskName, 30);
            PrintWordWithEmptySpace(Task.Duration.ToString(), 10);
            PrintWordWithEmptySpace(Task.StartDate.ToString("MM/dd/yyyy"), 13);
            PrintWordWithEmptySpace(Task.EndDate.ToString("MM/dd/yyyy"), 13);
            PrintWordWithEmptySpace(Task.Status, 13);
            PrintWordWithEmptySpace(Task.PercentageCompleted.ToString(), 21);
            PrintWordWithEmptySpace(Task.Priority, 10);
            PrintWordWithEmptySpace(Task.TaskWorkingHoursPerDay.ToString(), 18);
        }

        public void PrintTaskDescription(string TaskName)
        {
            Task? Task = FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot find description from task {TaskName}\n");
                return;
            }

            Print("Description:\n");
            foreach (string description in Task.Desription)
            {
                Print($"+ {description}\n");
            }
        }

        private void PrintInfoBarTitle()
        {
            Print("|");
            PrintWordWithEmptySpace("Task ID", 10);
            PrintWordWithEmptySpace("Task Name", 30);
            PrintWordWithEmptySpace("Duration", 10);
            PrintWordWithEmptySpace("Start Date", 13);
            PrintWordWithEmptySpace("Finish Date", 13);
            PrintWordWithEmptySpace("Status", 13);
            PrintWordWithEmptySpace("Percentage complete", 21);
            PrintWordWithEmptySpace("Priority", 10);
            PrintWordWithEmptySpace("Working hours/day", 18);
        }

        private void PrintResourceBarTitle(string TaskName, string TaskType, float TotalCost, string Currency)
        {
            string title = $"|                          {TaskType} {TaskName}";
            string costTitle = $"|                         Total cost: {TotalCost}{Currency}";

            Print("_____________________________________________________________________\n");
            PrintWordWithEmptySpace(title, 68); Print("\n");
            PrintWordWithEmptySpace(costTitle, 68); Print("\n");

            Print("|");
            PrintWordWithEmptySpace("Resource name", 21);
            PrintWordWithEmptySpace("Capacity", 11);
            PrintWordWithEmptySpace("Type", 10);
            PrintWordWithEmptySpace("Accure", 10);
            PrintWordWithEmptySpace("Cost", 11);
        }

        public void PrintWordWithEmptySpace(string word, int MaximumEmptySpace)
        {
            string space = "";
            int NumOfSpace = MaximumEmptySpace >= word.Length ? MaximumEmptySpace - word.Length : 0;

            for(int i = 1; i <= NumOfSpace; ++i)
            {
                space += " ";
            }

            Print($"{word}{space}|");
        }

        private void Print(string text)
        {
            Console.Write(text);
        }
    }

    class Vertex
    {
        public string TaskName;
        public int EarliestStart;
        public int EarliestEnd;
        public int LatestStart;
        public int LatestEnd;
        public int Duration;
        public int TotalFloat;

        public DateTime ES;
        public DateTime EF;
        public DateTime LS;
        public DateTime LF;

        public Dictionary<string, TypeLag> Depending_vertices = new Dictionary<string, TypeLag>();//TaskName - TypeLag
        public Dictionary<string, TypeLag> Depended_vertices = new Dictionary<string, TypeLag>(); //TaskName - TypeLag

        public Vertex(string name)
        {
            TaskName = name;
        }
    }

    class PDMDiGraph
    {
        public Dictionary<string, Vertex> vertices = new Dictionary<string, Vertex>();
        public Vertex Start = new Vertex("Start");
        public Vertex End = new Vertex("End");

        public PDMDiGraph()
        {
            Start.EarliestStart = -1;
            Start.EarliestEnd = -1;
        }

        public void ChangeVertexName(string vertex, string newVertex)
        {
            if (vertices.ContainsKey(vertex) && !vertices.ContainsKey(newVertex))
            {
                foreach (KeyValuePair<string, Vertex> v in vertices)
                {
                    if (v.Key != vertex)
                    {
                        if (v.Value.Depending_vertices.ContainsKey(vertex))
                        {
                            v.Value.Depending_vertices.Add(newVertex, v.Value.Depending_vertices[vertex]);
                            v.Value.Depending_vertices.Remove(vertex);
                        }

                        if (v.Value.Depended_vertices.ContainsKey(vertex))
                        {
                            v.Value.Depended_vertices.Add(newVertex, v.Value.Depended_vertices[vertex]);
                            v.Value.Depended_vertices.Remove(vertex);
                        }
                    }
                }

                vertices.Add(newVertex, vertices[vertex]);
                vertices[newVertex].TaskName = newVertex;
                vertices.Remove(vertex);
            }
            else Print($"Cannot change {vertex} to {newVertex} because vertex {vertex} does not exist or the vertex's new name {newVertex} already exist!\n");
        }

        public void CalculateStartAndEnd()
        {
            Start = new Vertex("Start");
            End = new Vertex("End");

            Start.EarliestStart = -1;
            Start.EarliestEnd = -1;

            List<Vertex> verticesWithoutPredecessor = new List<Vertex>();

            foreach (KeyValuePair<string, Vertex> vertex in vertices)
            {
                if (vertex.Value.Depending_vertices.Count == 0)
                {
                    if (vertex.Value.ES == DateTime.MinValue || vertex.Value.EF == DateTime.MinValue)
                    {
                        Print($"Timeline of task {vertex.Key} has not been set!\n");
                        Print($"Cannot begin the process of calculating Start Date and Finish Date of tasks.\n");
                        return;
                    }
                    else verticesWithoutPredecessor.Add(vertex.Value);
                }
            }

            DateTime minES = DateTime.MaxValue;

            foreach (Vertex v in verticesWithoutPredecessor)
            {
                if (v.ES < minES)
                {
                    minES = v.ES;
                }
            }

            Start.ES = minES.AddDays(-1);
            Start.EF = minES.AddDays(-1);

            vertices.Add("Start", Start);
            vertices.Add("End", End);

            foreach (KeyValuePair<string, Vertex> vertex in vertices)
            {
                if (vertex.Value.Depending_vertices.Count == 0 && vertex.Key != "Start")
                {
                    TimeSpan Lag = vertex.Value.ES - Start.EF;
                    AddEdge("Start", vertex.Key, "FS", Lag.Days - 1);
                }

                if (vertex.Value.Depended_vertices.Count == 0 && vertex.Key != "End") AddEdge(vertex.Key, "End", "FS");
            }

            Queue<string> TopoSorted = SortTopology();
            Stack<string> TopoSorted2 = new Stack<string>();

            foreach (string vertex in TopoSorted)
            {
                TopoSorted2.Push(vertex);
            }

            ForwardPass(TopoSorted);

            vertices["End"].LatestStart = vertices["End"].EarliestStart;
            vertices["End"].LatestEnd = vertices["End"].EarliestEnd;

            vertices["End"].LS = vertices["End"].ES;
            vertices["End"].LF = vertices["End"].EF;

            End.EarliestStart = vertices["End"].EarliestStart;
            End.EarliestEnd = vertices["End"].EarliestEnd;
            End.LatestStart = vertices["End"].LatestStart;
            End.LatestEnd = vertices["End"].LatestEnd;

            End.ES = vertices["End"].ES;
            End.EF = vertices["End"].EF;
            End.LS = vertices["End"].LS;
            End.LF = vertices["End"].LF;

            BackwardPass(TopoSorted2);

            --End.EarliestStart;
            --End.EarliestEnd;
            --End.LatestStart;
            --End.LatestEnd;

            End.ES = End.ES.AddDays(-1);
            End.EF = End.EF.AddDays(-1);
            End.LS = End.LS.AddDays(-1);
            End.LF = End.LF.AddDays(-1);

            RemoveVertex("Start");
            RemoveVertex("End");
        }

        private void ForwardPass(Queue<string> TopologySorted)
        {
            TopologySorted.Dequeue();

            while (TopologySorted.Count > 0)
            {
                string TaskName = TopologySorted.Dequeue();
                int MaxEarlyStart = int.MinValue;

                foreach (KeyValuePair<string, TypeLag> dependeingTask in vertices[TaskName].Depending_vertices)
                {
                    if (dependeingTask.Value.Type == "SS" && vertices[dependeingTask.Key].EarliestStart + dependeingTask.Value.Lag > MaxEarlyStart)
                    {
                        MaxEarlyStart = vertices[dependeingTask.Key].EarliestStart + dependeingTask.Value.Lag;
                        vertices[TaskName].ES = vertices[dependeingTask.Key].ES.AddDays(dependeingTask.Value.Lag);
                    }
                    else if (dependeingTask.Value.Type == "FF" && vertices[dependeingTask.Key].EarliestEnd + dependeingTask.Value.Lag - vertices[TaskName].Duration + 1 > MaxEarlyStart)
                    {
                        MaxEarlyStart = vertices[dependeingTask.Key].EarliestEnd + dependeingTask.Value.Lag - vertices[TaskName].Duration + 1;
                        vertices[TaskName].ES = vertices[dependeingTask.Key].EF.AddDays(dependeingTask.Value.Lag - vertices[TaskName].Duration + 1);
                    }
                    else if (dependeingTask.Value.Type == "FS" && vertices[dependeingTask.Key].EarliestEnd + 1 + dependeingTask.Value.Lag > MaxEarlyStart)
                    {
                        MaxEarlyStart = vertices[dependeingTask.Key].EarliestEnd + dependeingTask.Value.Lag + 1;
                        vertices[TaskName].ES = vertices[dependeingTask.Key].EF.AddDays(dependeingTask.Value.Lag + 1);
                    }
                    else if (dependeingTask.Value.Type == "SF" && vertices[dependeingTask.Key].EarliestStart + dependeingTask.Value.Lag - vertices[TaskName].Duration + 1 > MaxEarlyStart)
                    {
                        MaxEarlyStart = vertices[dependeingTask.Key].EarliestStart + dependeingTask.Value.Lag - vertices[TaskName].Duration + 1;
                        vertices[TaskName].ES = vertices[dependeingTask.Key].ES.AddDays(dependeingTask.Value.Lag - vertices[TaskName].Duration + 1);
                    }
                }

                vertices[TaskName].EarliestStart = MaxEarlyStart;

                if (TaskName == "End")
                {
                    vertices[TaskName].EarliestEnd = MaxEarlyStart + vertices[TaskName].Duration;
                    vertices[TaskName].EF = vertices[TaskName].ES.AddDays(vertices[TaskName].Duration);
                }
                else
                {
                    vertices[TaskName].EarliestEnd = MaxEarlyStart + vertices[TaskName].Duration - 1;
                    vertices[TaskName].EF = vertices[TaskName].ES.AddDays(vertices[TaskName].Duration - 1);
                }
            }
        }

        private void BackwardPass(Stack<string> TopologySorted)
        {
            TopologySorted.Pop();

            while (TopologySorted.Count > 0)
            {
                string TaskName = TopologySorted.Pop();
                int MinLatestEnd = int.MaxValue;

                foreach (KeyValuePair<string, TypeLag> dependedTask in vertices[TaskName].Depended_vertices)
                {
                    if (dependedTask.Value.Type == "SS" && vertices[dependedTask.Key].LatestStart - dependedTask.Value.Lag + vertices[TaskName].Duration - 1 < MinLatestEnd)
                    {
                        MinLatestEnd = vertices[dependedTask.Key].LatestStart - dependedTask.Value.Lag + vertices[TaskName].Duration - 1;
                        vertices[TaskName].LF = vertices[dependedTask.Key].LS.AddDays(-dependedTask.Value.Lag + vertices[TaskName].Duration - 1);
                    }
                    else if (dependedTask.Value.Type == "FF" && vertices[dependedTask.Key].LatestEnd - dependedTask.Value.Lag < MinLatestEnd)
                    {
                        MinLatestEnd = vertices[dependedTask.Key].LatestEnd - dependedTask.Value.Lag;
                        vertices[TaskName].LF = vertices[dependedTask.Key].LF.AddDays(-dependedTask.Value.Lag);
                    }
                    else if (dependedTask.Value.Type == "FS" && vertices[dependedTask.Key].LatestStart - 1 - dependedTask.Value.Lag < MinLatestEnd)
                    {
                        MinLatestEnd = vertices[dependedTask.Key].LatestStart - 1 - dependedTask.Value.Lag;
                        vertices[TaskName].LF = vertices[dependedTask.Key].LS.AddDays(-1 - dependedTask.Value.Lag);
                    }
                    else if (dependedTask.Value.Type == "SF" && vertices[dependedTask.Key].LatestEnd - dependedTask.Value.Lag + vertices[TaskName].Duration - 1 < MinLatestEnd)
                    {
                        MinLatestEnd = vertices[dependedTask.Key].LatestEnd - dependedTask.Value.Lag + vertices[TaskName].Duration - 1;
                        vertices[TaskName].LF = vertices[dependedTask.Key].LF.AddDays(-dependedTask.Value.Lag + vertices[TaskName].Duration - 1);
                    }
                }

                vertices[TaskName].LatestEnd = MinLatestEnd;

                if (TaskName == "Start")
                {
                    vertices[TaskName].LatestStart = MinLatestEnd - vertices[TaskName].Duration;
                    vertices[TaskName].LS = vertices[TaskName].LF.AddDays(-vertices[TaskName].Duration);
                }
                else
                {
                    vertices[TaskName].LatestStart = MinLatestEnd - vertices[TaskName].Duration + 1;
                    vertices[TaskName].LS = vertices[TaskName].LF.AddDays(-vertices[TaskName].Duration + 1);
                }

                vertices[TaskName].TotalFloat = vertices[TaskName].LatestStart - vertices[TaskName].EarliestStart;
            }
        }

        private Queue<string> SortTopology()
        {
            Dictionary<string, int> verticesWithNumofIncomingEdges = new Dictionary<string, int>();
            Queue<string> sortedVertices = new Queue<string>();

            foreach (KeyValuePair<string, Vertex> vertex in vertices)
            {
                verticesWithNumofIncomingEdges.Add(vertex.Key, vertex.Value.Depending_vertices.Count);
            }

            while (verticesWithNumofIncomingEdges.Count > 0)
            {
                foreach (KeyValuePair<string, int> vertex in verticesWithNumofIncomingEdges)
                {
                    if (vertex.Value == 0)
                    {
                        verticesWithNumofIncomingEdges.Remove(vertex.Key);
                        sortedVertices.Enqueue(vertex.Key);

                        foreach (KeyValuePair<string, Vertex> v in vertices)
                        {
                            if (v.Key != vertex.Key && v.Value.Depending_vertices.ContainsKey(vertex.Key))
                            {
                                --verticesWithNumofIncomingEdges[v.Key];
                            }
                        }

                        break;
                    }
                }
            }

            return sortedVertices;
        }

        public void AddDurationTo(string vertex, int duration)
        {
            if (vertices.ContainsKey(vertex)) vertices[vertex].Duration = duration;
            else Print($"Cannot add duration to vertex {vertex} because it does not exist!\n");
        }

        public void AddVertex(string TaskName)
        {
            if (!CheckIfVertexExists(TaskName))
            {
                vertices.Add(TaskName, new Vertex(TaskName));
            }
        }

        public void RemoveVertex(string TaskName)
        {
            if (CheckIfVertexExists(TaskName))
            {
                vertices.Remove(TaskName);

                foreach (KeyValuePair<string, Vertex> vertex in vertices)
                {
                    if (vertex.Value.Depended_vertices.ContainsKey(TaskName))
                    {
                        vertex.Value.Depended_vertices.Remove(TaskName);
                    }

                    if (vertex.Value.Depending_vertices.ContainsKey(TaskName))
                    {
                        vertex.Value.Depending_vertices.Remove(TaskName);
                    }
                }
            }
            else Print($"Cannot remove vertex {TaskName} because it does not exist!\n");
        }

        public void AddEdge(string v1, string v2, string Type, int Lag = 0)
        {
            AddVertex(v1);
            AddVertex(v2);

            if (!CheckIfEdgeExists(v1, v2))
            {
                vertices[v1].Depended_vertices.Add(v2, new TypeLag(Type));
                vertices[v2].Depending_vertices.Add(v1, new TypeLag(Type));
                vertices[v1].Depended_vertices[v2].Lag = Lag;
                vertices[v2].Depending_vertices[v1].Lag = Lag;
            }
            else
            {
                if (vertices[v1].Depended_vertices[v2].Type != Type)
                {
                    vertices[v1].Depended_vertices[v2].Type = Type;
                    vertices[v2].Depending_vertices[v1].Type = Type;
                }

                if (vertices[v1].Depended_vertices[v2].Lag != Lag)
                {
                    vertices[v1].Depended_vertices[v2].Lag = Lag;
                    vertices[v2].Depending_vertices[v1].Lag = Lag;
                }
            }
        }

        public void RemoveEdge(string v1, string v2)
        {
            if (CheckIfEdgeExists(v1, v2))
            {
                vertices[v1].Depended_vertices.Remove(v2);
                vertices[v2].Depending_vertices.Remove(v1);
            }
            else Print($"Cannot remove edge {v1} -> {v2} because it does not exist!\n");
        }

        public bool CheckIfVertexExists(string vertex)
        {
            return vertices.ContainsKey(vertex);
        }

        public bool CheckIfEdgeExists(string v1, string v2)
        {
            return vertices[v1].Depended_vertices.ContainsKey(v2);
        }

        public bool CheckingLoop()
        {
            return CheckingLoop2(vertices.ElementAt(0).Key, new List<string>());
        }

        private bool CheckingLoop2(string expandVertex, List<string> visited)
        {
            if (visited.Contains(expandVertex))
            {
                Print($"The sequence of dependencies that creates loop: ");
                foreach (string vertex in visited)
                {
                    Print($"{vertex} -> ");
                }
                Print($"{expandVertex}\n");

                return true;
            }
            else
            {
                visited.Add(expandVertex);

                foreach (KeyValuePair<string, TypeLag> neighbor in vertices[expandVertex].Depended_vertices)
                {
                    if (CheckingLoop2(neighbor.Key, visited))
                    {
                        return true;
                    }
                }

                visited.Remove(expandVertex);
                return false;
            }
        }

        public void PrintAllVerticesInfo()
        {
            //PrintVertexInfo(Start); Print("\n");
            foreach (KeyValuePair<string, Vertex> vertex in vertices)
            {
                PrintVertexInfo(vertex.Value);
                Print("\n");
            }
            //PrintVertexInfo(End); Print("\n");
        }

        public void PrintVertexInfo(Vertex vertex)
        {
            Print($"vertex: {vertex.TaskName}\n");
            Print($"Duration = {vertex.Duration}\n");
            Print($"{vertex.TaskName}.ES = {vertex.EarliestStart}\n");
            Print($"{vertex.TaskName}.EF = {vertex.EarliestEnd}\n");
            Print($"{vertex.TaskName}.LS = {vertex.LatestStart}\n");
            Print($"{vertex.TaskName}.LF = {vertex.LatestEnd}\n");
            Print($"Earliest Start Date: {vertex.ES.ToString("MM/dd/yyyy")}\n");
            Print($"Earliest Finish Date: {vertex.EF.ToString("MM/dd/yyyy")}\n");
            Print($"Latest Start Date: {vertex.LS.ToString("MM/dd/yyyy")}\n");
            Print($"Latest Finish Date: {vertex.LF.ToString("MM/dd/yyyy")}\n");
            Print($"Total Float = {vertex.TotalFloat}\n");

            Print("Depending: ");
            foreach (KeyValuePair<string, TypeLag> v in vertex.Depending_vertices)
            {
                Print($"{v.Key}-{v.Value.Type}{v.Value.Lag} ");
            }

            Print("\nDepended: ");
            foreach (KeyValuePair<string, TypeLag> v in vertex.Depended_vertices)
            {
                Print($"{v.Key}-{v.Value.Type}{v.Value.Lag} ");
            }
            Print("\n");
        }

        public void PrintTopologyInfo()
        {
            Queue<string> Topology = SortTopology();

            Print("Topo sort: ");
            foreach (string vertex in Topology)
            {
                Print($"{vertex} ");
            }
            Print("\n");
        }

        private void Print(string text)
        {
            Console.Write(text);
        }
    }

    class GanttChartBar
    {
        public string TaskName;

        public DateTime InitialStartDate;
        public DateTime StartDate; //
        public DateTime InitialFinishDate;
        public DateTime FinishDate; //
        public int Duration;

        public string Priority = "";
        public int BarLevel;
        public string Status = "Not start";
        public int PercentageCompleted;
        public bool Critical = false;
        public int TotalFloat;

        public Dictionary<string, int> ResourceAndCapacity = new Dictionary<string, int>();

        public Dictionary<string, TypeLag> InitialDepending_vertices = new Dictionary<string, TypeLag>();
        public Dictionary<string, TypeLag> Depending_vertices = new Dictionary<string, TypeLag>(); //

        public Dictionary<string, TypeLag> InitialDepended_vertices = new Dictionary<string, TypeLag>();
        public Dictionary<string, TypeLag> Depended_vertices = new Dictionary<string, TypeLag>(); //

        public GanttChartBar(string name)
        {
            TaskName = name;
        }
    }

    class GanttChart : IPrintWordWithEmptySpace
    {
        public PDMDiGraph Graph = new PDMDiGraph();
        private TreeOfTasks Tree;
        public List<string> TasksWithOrder = new List<string>();
        public Dictionary<string, GanttChartBar> TaskBars = new Dictionary<string, GanttChartBar>();
        private int LowestBarLevel;

        public GanttChart(TreeOfTasks Tree)
        {
            this.Tree = Tree;
            bool HasDependencies = true;
            List<string> TopoSort = new List<string>();

            if (Tree.Dependencies.Count == 0)
            {
                HasDependencies = false;
                Print("No dependencies found!\n");
            }
            else
            {
                BuildPDMGraph();
                if (Graph.vertices.Count == 0) return;
                else TopoSort = TopoSortForChart();
            }

            Print("Array of tasks before adding subtasks without dependencies: ");
            foreach (string task in TopoSort)
            {
                Print($"{task} ");
            }
            Print("\n\n");

            BuildGanttChart(HasDependencies, TopoSort);
        }

        private void BuildGanttChart(bool HasDependencies, List<string> TopoSort)
        {
            LinkedList<string> tasksWithOrder = new LinkedList<string>(TopoSort);
            List<string> subtasksWithoutDependency = SubtasksWithoutDependency();

            foreach (string subtask in subtasksWithoutDependency)
            {
                AddSubtaskWithoutDependencyToTopo(subtask, tasksWithOrder, HasDependencies);
            }

            string[] array = new string[tasksWithOrder.Count];
            tasksWithOrder.CopyTo(array, 0);

            int level = FindLongestIDlength(tasksWithOrder) - 1;
            LowestBarLevel = level + 1;
            Dictionary<int, List<string>> levelWithTasks = LevelInTreeWithItsTasksExceptSubtasks();

            Print("Array of tasks after adding subtasks without dependencies: ");
            foreach (string task in array)
            {
                if(!TopoSort.Contains(task))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Print($"{task} ");
                    Console.ResetColor();
                }
                else Print($"{task} ");
            }
            Print("\n\n");

            while (level > 0)
            {
                foreach (string task in levelWithTasks[level])
                {
                    array = AddSummaryTaskToTopo(task, array);

                    Print($"Tasks in array after adding task {task}: ");
                    foreach (string taskArray in array)
                    {
                        if(taskArray == task)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Print($"{taskArray} ");
                            Console.ResetColor();
                        }
                        else Print($"{taskArray} ");
                    }
                    Print("\n\n");
                }

                --level;
            }

            CreateTasksOrderAndTaskBars(array);
        }

        private void CreateTasksOrderAndTaskBars(string[] ArrayOfTasksWithOrder)
        {
            TasksWithOrder = new List<string>(ArrayOfTasksWithOrder);
            AddTaskToTaskBars(ArrayOfTasksWithOrder);
            BuildTaskBars();
        }

        private void AddTaskToTaskBars(string[] ArrayOfTasksWithOrder)
        {
            foreach (string task in ArrayOfTasksWithOrder)
            {
                if (!TaskBars.ContainsKey(task))
                {
                    TaskBars.Add(task, new GanttChartBar(task));
                }
            }
        }

        private void BuildTaskBars()
        {
            foreach (KeyValuePair<string, GanttChartBar> task in TaskBars)
            {
                Task? TaskNode = Tree.FindTaskNode(task.Key);
                if (TaskNode == null)
                {
                    Print($"Cannot build task bar because task {task.Key} does not exist!\n");
                    return;
                }

                task.Value.Duration = TaskNode.Duration;
                task.Value.Priority = TaskNode.Priority;
                task.Value.BarLevel = TaskNode.TaskNodeLevelInTree;
                task.Value.Status = TaskNode.Status;
                task.Value.PercentageCompleted = TaskNode.PercentageCompleted;
                task.Value.ResourceAndCapacity = TaskNode.ResourceAndCapacityDic;

                task.Value.InitialStartDate = TaskNode.StartDate;
                task.Value.InitialFinishDate = TaskNode.EndDate;

                if (Tree.graph.vertices.ContainsKey(task.Key))
                {
                    task.Value.InitialDepending_vertices = Tree.graph.vertices[task.Key].Depending_vertices;
                    task.Value.InitialDepended_vertices = Tree.graph.vertices[task.Key].Depended_vertices;
                }

                if (Graph.vertices.ContainsKey(task.Key))
                {
                    task.Value.StartDate = Graph.vertices[task.Key].ES;
                    task.Value.FinishDate = Graph.vertices[task.Key].EF;

                    task.Value.Depending_vertices = Graph.vertices[task.Key].Depending_vertices;
                    task.Value.Depended_vertices = Graph.vertices[task.Key].Depended_vertices;
                }
                else
                {
                    task.Value.StartDate = task.Value.InitialStartDate;
                    task.Value.FinishDate = task.Value.InitialFinishDate;

                    task.Value.Depending_vertices = task.Value.InitialDepending_vertices;
                    task.Value.Depended_vertices = task.Value.InitialDepended_vertices;
                }

                if (Graph.CheckIfVertexExists(task.Key))
                {
                    task.Value.TotalFloat = Graph.vertices[task.Key].TotalFloat;

                    if (task.Value.TotalFloat == 0)
                    {
                        task.Value.Critical = true;
                    }
                }
            }
        }

        public static bool CanBuildGanttChart(TreeOfTasks Tree)
        {
            bool build = true;

            if(Tree.TaskNameandIDDic.Count == 1)
            {
                Console.Write("Cannot create Gantt Chart because no task exists!\n");
                return false;
            }

            foreach (KeyValuePair<string, string> Task in Tree.TaskNameandIDDic)
            {
                if (!Tree.AlreadySettingTimeline(Task.Key))
                {
                    build = false;
                    Console.Write($"Timeline of {Task.Key} has not been set!\n");
                    Console.Write("Cannot create Gantt Chart!\n");
                    break;
                }
            }

            return build;
        }

        private void BuildPDMGraph()
        {
            Graph = new PDMDiGraph();

            foreach (KeyValuePair<string, DependencyTaskInfo> dependency in Tree.Dependencies)
            {
                List<string> leafTasks = Tree.FindLeafTasksOf(dependency.Key);
                List<string> earliestLeafTasks = new List<string>();
                List<string> latestLeafTasks = new List<string>();

                if (leafTasks.Count == 1)
                {
                    earliestLeafTasks.Add(leafTasks[0]);
                    latestLeafTasks.Add(leafTasks[0]);
                }
                else
                {
                    earliestLeafTasks = Tree.EarliestTasks(leafTasks);
                    latestLeafTasks = Tree.LatestTasks(leafTasks);
                }

                foreach (KeyValuePair<string, TypeLag> depending in dependency.Value.DependingTasks)
                {
                    List<string> dependingLeafTasks = Tree.FindLeafTasksOf(depending.Key);
                    List<string> earliestDependingLeafTasks = new List<string>();
                    List<string> latestDependingLeafTasks = new List<string>();

                    string Type = depending.Value.Type;
                    int Lag = depending.Value.Lag;

                    if (dependingLeafTasks.Count == 1)
                    {
                        earliestDependingLeafTasks.Add(dependingLeafTasks[0]);
                        latestDependingLeafTasks.Add(dependingLeafTasks[0]);
                    }
                    else
                    {
                        earliestDependingLeafTasks = Tree.EarliestTasks(dependingLeafTasks);
                        latestDependingLeafTasks = Tree.LatestTasks(dependingLeafTasks);
                    }

                    if (Type == "SS")
                    {
                        foreach (string Task in earliestLeafTasks)
                        {
                            Task? TaskNode = Tree.FindTaskNode(Task);
                            if (TaskNode == null)
                            {
                                Print($"Cannot find task {Task} in Tree!\n");
                                Print("Cannot create Gantt Chart!\n");
                                Graph = new PDMDiGraph();
                                return;
                            }

                            foreach (string DependingTask in earliestDependingLeafTasks)
                            {
                                Task? DependingTaskNode = Tree.FindTaskNode(DependingTask);
                                if (DependingTaskNode == null)
                                {
                                    Print($"Cannot find task {DependingTask} in Tree!\n");
                                    Print("Cannot create Gantt Chart!\n");
                                    Graph = new PDMDiGraph();
                                    return;
                                }

                                Graph.AddEdge(DependingTask, Task, "SS", Lag);
                                Graph.AddDurationTo(DependingTask, DependingTaskNode.Duration);
                            }

                            Graph.AddDurationTo(Task, TaskNode.Duration);
                        }
                    }
                    else if (Type == "FF")
                    {
                        foreach (string Task in latestLeafTasks)
                        {
                            Task? TaskNode = Tree.FindTaskNode(Task);
                            if (TaskNode == null)
                            {
                                Print($"Cannot find task {Task} in Tree!\n");
                                Print("Cannot create Gantt Chart!\n");
                                Graph = new PDMDiGraph();
                                return;
                            }

                            foreach (string DependingTask in latestDependingLeafTasks)
                            {
                                Task? DependingTaskNode = Tree.FindTaskNode(DependingTask);
                                if (DependingTaskNode == null)
                                {
                                    Print($"Cannot find task {DependingTask} in Tree!\n");
                                    Print("Cannot create Gantt Chart!\n");
                                    Graph = new PDMDiGraph();
                                    return;
                                }

                                Graph.AddEdge(DependingTask, Task, "FF", Lag);
                                Graph.AddDurationTo(DependingTask, DependingTaskNode.Duration);
                            }

                            Graph.AddDurationTo(Task, TaskNode.Duration);
                        }
                    }
                    else if (Type == "FS")
                    {
                        foreach (string Task in earliestLeafTasks)
                        {
                            Task? TaskNode = Tree.FindTaskNode(Task);
                            if (TaskNode == null)
                            {
                                Print($"Cannot find task {Task} in Tree!\n");
                                Print("Cannot create Gantt Chart!\n");
                                Graph = new PDMDiGraph();
                                return;
                            }

                            foreach (string DependingTask in latestDependingLeafTasks)
                            {
                                Task? DependingTaskNode = Tree.FindTaskNode(DependingTask);
                                if (DependingTaskNode == null)
                                {
                                    Print($"Cannot find task {DependingTask} in Tree!\n");
                                    Print("Cannot create Gantt Chart!\n");
                                    Graph = new PDMDiGraph();
                                    return;
                                }

                                Graph.AddEdge(DependingTask, Task, "FS", Lag);
                                Graph.AddDurationTo(DependingTask, DependingTaskNode.Duration);
                            }

                            Graph.AddDurationTo(Task, TaskNode.Duration);
                        }
                    }
                    else if (Type == "SF")
                    {
                        foreach (string Task in latestLeafTasks)
                        {
                            Task? TaskNode = Tree.FindTaskNode(Task);
                            if (TaskNode == null)
                            {
                                Print($"Cannot find task {Task} in Tree!\n");
                                Print("Cannot create Gantt Chart!\n");
                                Graph = new PDMDiGraph();
                                return;
                            }

                            foreach (string DependingTask in earliestDependingLeafTasks)
                            {
                                Task? DependingTaskNode = Tree.FindTaskNode(DependingTask);
                                if (DependingTaskNode == null)
                                {
                                    Print($"Cannot find task {DependingTask} in Tree!\n");
                                    Print("Cannot create Gantt Chart!\n");
                                    Graph = new PDMDiGraph();
                                    return;
                                }

                                Graph.AddEdge(DependingTask, Task, "SF", Lag);
                                Graph.AddDurationTo(DependingTask, DependingTaskNode.Duration);
                            }

                            Graph.AddDurationTo(Task, TaskNode.Duration);
                        }
                    }

                    if (Graph.CheckingLoop())
                    {
                        Print("Cannot create Gantt Chart because of loop!\n");
                        Graph = new PDMDiGraph();
                        return;
                    }
                }
            }

            foreach (KeyValuePair<string, Vertex> vertex in Graph.vertices)
            {
                if (vertex.Value.Depending_vertices.Count == 0)
                {
                    Task? Task = Tree.FindTaskNode(vertex.Key);
                    if (Task == null)
                    {
                        Print($"Cannot find task {vertex.Key} in Tree!\n");
                        Graph = new PDMDiGraph();
                        return;
                    }

                    vertex.Value.ES = Task.StartDate;
                    vertex.Value.EF = Task.EndDate;
                }
            }

            Graph.CalculateStartAndEnd();
        }

        private string[] AddSummaryTaskToTopo(string TaskName, string[] array)
        {
            string TaskID = Tree.GetTaskID(TaskName);
            List<int> indices = new List<int>();
            bool repeat = false;

            for (int i = 0, plus = 0; i < array.Length; ++i)
            {
                string ID = Tree.GetTaskID(array[i]);

                if (ID.Length > TaskID.Length && TaskID == ID.Substring(0, TaskID.Length) && !repeat) 
                {
                    indices.Add(i + plus);
                    ++plus;
                    repeat = true;
                }
                else if (!(ID.Length > TaskID.Length && TaskID == ID.Substring(0, TaskID.Length) && repeat)) repeat = false;
            }

            string[] newArray = new string[array.Length + indices.Count];

            foreach (int index in indices)
            {
                newArray[index] = TaskName;
            }

            for (int i = 0, j = 0; j < array.Length; ++i, ++j)
            {
                if (newArray[i] == null) newArray[i] = array[j];
                else
                {
                    while (newArray[i] != null) ++i;
                    newArray[i] = array[j];
                }
            }

            return newArray;
        }

        private List<string> SubtasksWithoutDependency()
        {
            List<string> tasks = new List<string>();

            foreach (KeyValuePair<string, TaskIDandSubtasksID> ID in Tree.TaskIDandSubTaskIDDic)
            {
                string TaskName = Tree.GetTaskNameFromID(ID.Key);

                if (ID.Value.SubTaskIDS.Count == 0 && !Graph.vertices.ContainsKey(TaskName))
                {
                    tasks.Add(TaskName);
                }
            }

            return tasks;
        }

        private Dictionary<int, List<string>> LevelInTreeWithItsTasksExceptSubtasks()
        {
            Dictionary<int, List<string>> dic = new Dictionary<int, List<string>>();

            foreach (KeyValuePair<string, TaskIDandSubtasksID> ID in Tree.TaskIDandSubTaskIDDic)
            {
                if (ID.Value.SubTaskIDS.Count > 0 && ID.Key != "0")
                {
                    int IDlength = ID.Key.Length;

                    if (!dic.ContainsKey(IDlength))
                    {
                        dic.Add(IDlength, new List<string>());
                    }

                    dic[IDlength].Add(Tree.GetTaskNameFromID(ID.Key));
                }
            }

            return dic;
        }

        private void AddSubtaskWithoutDependencyToTopo(string TaskName, LinkedList<string> linkedList, bool HasDependencies = true)
        {
            string chosen = "";
            Task? Task = Tree.FindTaskNode(TaskName);
            if (Task == null)
            {
                Print($"Cannot add task {TaskName} to Topology sort because it does not exist!\n");
                return;
            }

            foreach (string s in linkedList)
            {
                if (HasDependencies && Task.StartDate < Graph.vertices[s].ES)
                {
                    chosen = s;
                    break;
                }
                else if (!HasDependencies)
                {
                    Task? linkedListTask = Tree.FindTaskNode(s);
                    if (linkedListTask == null)
                    {
                        Print($"Error! Task {s} is not in Tree!\n");
                        return;
                    }

                    if (Task.StartDate < linkedListTask.StartDate)
                    {
                        chosen = s;
                        break;
                    }
                }
            }

            if (chosen == "") linkedList.AddLast(TaskName);
            else
            {
                LinkedListNode<string>? current = linkedList.Find(chosen);
                if (current == null)
                {
                    Print($"Cannot find the chosen task {chosen}!\n");
                    return;
                }
                else linkedList.AddBefore(current, TaskName);
            }
        }

        private int FindLongestIDlength(LinkedList<string> listOfTasks)
        {
            List<int> nums = new List<int>();

            foreach (string task in listOfTasks)
            {
                int length = Tree.GetTaskID(task).Length;

                if (!nums.Contains(length))
                {
                    nums.Add(length);
                }
            }

            return nums.Max();
        }

        private List<string> TopoSortForChart()
        {
            List<string> Sort = new List<string>();
            List<string> Tasks = new List<string>();

            foreach (KeyValuePair<string, Vertex> vertex in Graph.vertices)
            {
                if (vertex.Value.Depending_vertices.Count == 0)
                {
                    Tasks.Add(vertex.Key);
                }
            }

            string RecentTask = Tree.EarliestTasks(Tasks)[0];
            DateTime EarliestStart = Graph.vertices[RecentTask].ES;
            Dictionary<string, int> verticesWithNumofIncomingEdges = new Dictionary<string, int>();

            foreach (KeyValuePair<string, Vertex> vertex in Graph.vertices)
            {
                verticesWithNumofIncomingEdges.Add(vertex.Key, vertex.Value.Depending_vertices.Count);
            }

            verticesWithNumofIncomingEdges.Remove(RecentTask);

            foreach (KeyValuePair<string, Vertex> v in Graph.vertices)
            {
                if (v.Key != RecentTask && v.Value.Depending_vertices.ContainsKey(RecentTask))
                {
                    --verticesWithNumofIncomingEdges[v.Key];
                }
            }

            Sort.Add(RecentTask);

            while (verticesWithNumofIncomingEdges.Count > 0)
            {
                List<Vertex> verticesWithNoPredecessors = new List<Vertex>();

                foreach (KeyValuePair<string, int> vertex in verticesWithNumofIncomingEdges)
                {
                    if (vertex.Value == 0)
                    {
                        verticesWithNoPredecessors.Add(Graph.vertices[vertex.Key]);
                    }
                }

                string ChosenTask = ReturnTaskWithLowestHC(verticesWithNoPredecessors, RecentTask, EarliestStart);
                Sort.Add(ChosenTask);
                verticesWithNumofIncomingEdges.Remove(ChosenTask);
                RecentTask = ChosenTask;

                foreach (KeyValuePair<string, Vertex> v in Graph.vertices)
                {
                    if (v.Key != ChosenTask && v.Value.Depending_vertices.ContainsKey(ChosenTask))
                    {
                        --verticesWithNumofIncomingEdges[v.Key];
                    }
                }
            }

            return Sort;
        }

        private string ReturnTaskWithLowestHC(List<Vertex> vertices, string RecentTaskName, DateTime EarliestStartOfAll)
        {
            string Task = "";
            int minValue = int.MaxValue;

            foreach (Vertex vertex in vertices)
            {
                int value = CalculateHeuristic(vertex.TaskName, RecentTaskName, EarliestStartOfAll);

                if (value < minValue)
                {
                    minValue = value;
                    Task = vertex.TaskName;
                }
            }

            if (vertices.Count == 0) Print($"Warning! Return an empty task string!\n");
            return Task;
        }

        private int CalculateHeuristic(string TaskName, string RecentTaskName, DateTime EarliestStartOfAll)
        {
            string TaskID = Tree.GetTaskID(TaskName);
            string RecentTaskID = Tree.GetTaskID(RecentTaskName);
            if (TaskID == "" || RecentTaskID == "") Print($"Warning! Task {TaskName} or {RecentTaskName} does not exist!\n");

            int TaskIDtoInt = TaskID.Length == 1 ? 0 : int.Parse(TaskID.Substring(0, TaskID.Length - 1));
            int RecentTaskIDtoInt = RecentTaskID.Length == 1 ? 0 : int.Parse(RecentTaskID.Substring(0, RecentTaskID.Length - 1));

            TimeSpan span = Graph.vertices[TaskName].ES - EarliestStartOfAll;
            int value = Math.Abs(TaskIDtoInt - RecentTaskIDtoInt) + span.Days;
            return value;
        }

        public void PrintDetailedInfoOfAllTasks()
        {
            PrintTasksOrderWithBasicInfo();Print("\n\n");

            foreach(KeyValuePair<string, GanttChartBar> Task in TaskBars)
            {
                PrintDependencyInfoOfTask(Task.Key); Print("\n\n");
            }

            PrintResourceTaskBarTitle(); Print("\n");
            foreach (KeyValuePair<string, GanttChartBar> Task in TaskBars)
            {
                ResourceInfoOfTask(Task.Key); Print("\n");
            }
        }

        public void PrintTasksOrderWithBasicInfo()
        {
            PrintBasicTaskBarTitle(); Print("\n");

            foreach (string task in TasksWithOrder)
            {
                string emptySpace = "";
                int IDLength = Tree.GetTaskID(task).Length;

                for (int i = 1; i <= IDLength - 1; ++i)
                {
                    emptySpace += " ";
                }

                Print($"|{emptySpace}");
                BasicInfoOfTask(task); Print("\n");
            }
        }

        public void PrintBasicInfoOfTask(string TaskName)
        {
            PrintBasicTaskBarTitle(); Print("\n|");
            BasicInfoOfTask(TaskName); Print("\n");
        }

        private void BasicInfoOfTask(string TaskName)
        {
            GanttChartBar TaskBar = TaskBars[TaskName];
            int bonusLength = LowestBarLevel - Tree.GetTaskID(TaskName).Length;

            if(Graph.vertices.ContainsKey(TaskName))
            {
                if(TaskBar.Critical) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.Blue;
            }

            PrintWordWithEmptySpace(TaskBar.TaskName, 30 + bonusLength);
            PrintWordWithEmptySpace(TaskBar.Duration.ToString(), 10);
            PrintWordWithEmptySpace(TaskBar.StartDate.ToString("MM/dd/yyyy"), 13);
            PrintWordWithEmptySpace(TaskBar.FinishDate.ToString("MM/dd/yyyy"), 13);
            PrintWordWithEmptySpace(TaskBar.Status, 13);
            PrintWordWithEmptySpace(TaskBar.Priority, 10);
            PrintWordWithEmptySpace(TaskBar.PercentageCompleted.ToString(), 21);

            if (Graph.vertices.ContainsKey(TaskName)) PrintWordWithEmptySpace(TaskBar.TotalFloat.ToString(), 12);
            else PrintWordWithEmptySpace("           ", 12);

            Console.ResetColor();
        }

        public void PrintDependencyInfoOfTask(string TaskName)
        {
            int len = 58;
            GanttChartBar TaskBar = TaskBars[TaskName];

            string InitialPreString = "";
            string CurrentPreString = "";
            string InitialSucString = "";
            string CurrentSucString = "";

            foreach (KeyValuePair<string, TypeLag> dependingTask in TaskBar.InitialDepending_vertices)
            {
                InitialPreString += dependingTask.Key + "-" + dependingTask.Value.Type + dependingTask.Value.Lag + " ";
            }

            foreach (KeyValuePair<string, TypeLag> dependingTask in TaskBar.Depending_vertices)
            {
                CurrentPreString += dependingTask.Key + "-" + dependingTask.Value.Type + dependingTask.Value.Lag + " ";
            }

            foreach (KeyValuePair<string, TypeLag> dependedTask in TaskBar.InitialDepended_vertices)
            {
                InitialSucString += dependedTask.Key + "-" + dependedTask.Value.Type + dependedTask.Value.Lag + " ";
            }

            foreach (KeyValuePair<string, TypeLag> dependedTask in TaskBar.Depended_vertices)
            {
                CurrentSucString += dependedTask.Key + "-" + dependedTask.Value.Type + dependedTask.Value.Lag + " ";
            }

            if (InitialPreString == "") InitialPreString = "none";
            if (CurrentPreString == "") CurrentPreString = "none";
            if (InitialSucString == "") InitialSucString = "none";
            if (CurrentSucString == "") CurrentSucString = "none";

            Print("_______________________________________________________________________________________________________________________\n");
            string title = $"|                                                         {TaskName}";

            PrintWordWithEmptySpace(title, len * 2 + 2);
            Print("\n|");
            PrintWordWithEmptySpace("Initial predecessors", len);
            PrintWordWithEmptySpace("Current predecessors", len);

            Print("\n|");
            PrintWordWithEmptySpace(InitialPreString, len);
            PrintWordWithEmptySpace(CurrentPreString, len);

            Print("\n|");
            PrintWordWithEmptySpace("Initial successors", len);
            PrintWordWithEmptySpace("Current successors", len);

            Print("\n|");
            PrintWordWithEmptySpace(InitialSucString, len);
            PrintWordWithEmptySpace(CurrentSucString, len); Print("\n");
        }

        public void PrintResourceInfoOfTask(string TaskName)
        {
            PrintResourceTaskBarTitle(); Print("\n");
            ResourceInfoOfTask(TaskName); Print("\n");
        }

        private void ResourceInfoOfTask(string TaskName)
        {
            GanttChartBar TaskBar = TaskBars[TaskName];
            string ResourceString = "";

            foreach(KeyValuePair<string, int> resource in TaskBar.ResourceAndCapacity)
            {
                ResourceString += resource.Key + "-" + resource.Value + " ";
            }

            Print("|");
            PrintWordWithEmptySpace(TaskName, 30);
            PrintWordWithEmptySpace(ResourceString, 87);
        }

        private void PrintBasicTaskBarTitle()
        {
            Print("_____________________________________________________________________________________________________________________________________\n");
            Print("|");
            PrintWordWithEmptySpace("Task Name", 30 + LowestBarLevel - 1);
            PrintWordWithEmptySpace("Duration", 10);
            PrintWordWithEmptySpace("Start Date", 13);
            PrintWordWithEmptySpace("Finish Date", 13);
            PrintWordWithEmptySpace("Status", 13);
            PrintWordWithEmptySpace("Priority", 10);
            PrintWordWithEmptySpace("Percentage complete", 21);
            PrintWordWithEmptySpace("Total float", 12);
        }

        private void PrintResourceTaskBarTitle()
        {
            Print("________________________________________________________________________________________________________________________\n");
            Print("|");
            PrintWordWithEmptySpace("Task Name", 30);
            PrintWordWithEmptySpace("Resource and Capacity", 87);
        }

        public void PrintWordWithEmptySpace(string word, int MaximumEmptySpace)
        {
            string space = "";
            int NumOfSpace = MaximumEmptySpace >= word.Length ? MaximumEmptySpace - word.Length : 0;

            for (int i = 1; i <= NumOfSpace; ++i)
            {
                space += " ";
            }

            Print($"{word}{space}|");
        }

        private void Print(string text)
        {
            Console.Write(text);
        }
    }

    class ProjectManagement
    {
        public TreeOfTasks ProjectTree;
        public GanttChart? ProjectGanttChart;
        public ResourceManagement Resources = new ResourceManagement();

        public ProjectManagement(string ProjectName)
        {
            ProjectTree = new TreeOfTasks(ProjectName);
        }

        public void SetCurrenDateOfProject(DateTime ProjectStatusDate)
        {
            if (ProjectStatusDate != DateTime.MinValue)
            {
                ProjectTree.CurrentDate = ProjectStatusDate;
            }
        }

        public void CreateOrUpdateGanttChart()
        {
            if (GanttChart.CanBuildGanttChart(ProjectTree))
            {
                ProjectGanttChart = new GanttChart(ProjectTree);
                if(ProjectGanttChart.TasksWithOrder.Count == 0) Print("Create/Update Gantt Chart failed!\n");
            }
            else Print("Create/Update Gantt Chart failed!\n");
        }

        public void AddTask(string TaskName)
        {
            if (TaskName != "Start" && TaskName != "End") ProjectTree.AddTaskToRootTask(TaskName);
            else Print("Cannot add task with name Start or End!\n");
        }

        public void AddSubtaskToTask(string SubtaskName, string TaskName)
        {
            if(SubtaskName != "Start" && SubtaskName != "End") ProjectTree.AddSubtaskToTask(SubtaskName, TaskName); 
            else Print("Cannot add subtask with name Start or End!\n");
        }

        public void ChangeTaskName(string CurrentTaskName, string NewTaskName)
        {
            if (CurrentTaskName != ProjectTree.RootTask.TaskName && NewTaskName != "Start" && NewTaskName != "End") ProjectTree.ChangeTaskName(CurrentTaskName, NewTaskName);
            else if (NewTaskName == "Start" || NewTaskName == "End") Print($"Cannot change name to Start or End!\n");
            else Print("Cannot change Project's name!\n");
        }

        public void DeleteTask(string TaskName)
        {
            ProjectTree.DeleteTask(TaskName);
        }

        public void AddDependency(string TaskName, string DependingTaskName, string DependencyType)
        {
            ProjectTree.AddDependency(TaskName, DependingTaskName, DependencyType);
        }

        public void AddLagToDependency(string TaskName, string DependingTaskName, int Lag)
        {
            ProjectTree.AddLagToDependency(TaskName, DependingTaskName, Lag);
        }

        public void DeleteDependency(string TaskName, string DependingTaskName)
        {
            ProjectTree.DeleteDependency(TaskName, DependingTaskName);
        }

        public void SetTimelineOfTask(string TaskName, DateTime StartDate, DateTime FinishDate)
        {
            ProjectTree.SetTimelineForTask(TaskName, StartDate, FinishDate);
        }

        public void ChangeDurationOfTask(string TaskName, int Duration)
        {
            ProjectTree.ChangeDuration(TaskName, Duration);
        }

        public void UpdateStatusOfTask(string TaskName, string Status)
        {
            ProjectTree.UpdateStatus(TaskName, Status);
        }

        public void AddDescriptionToTask(string TaskName, string Description)
        {
            ProjectTree.AddDescriptionToTask(TaskName, Description);
        }

        public void DeleteTaskDescriptionNumber(string TaskName, int Number)
        {
            ProjectTree.DeleteDescriptionOfTaskAtLine(TaskName, Number);
        }

        public void DeleteAllDescriptionOfTask(string TaskName)
        {
            ProjectTree.DeleteAllDescriptionOfTask(TaskName);
        }

        public void SetPriorityOfTask(string TaskName, string Priority)
        {
            ProjectTree.SetPriorityOfTask(TaskName, Priority);
        }

        public void SetNumberOfWorkingHoursPerDayOfTask(string TaskName, float Hours)
        {
            ProjectTree.SetWorkingHoursPerDayFOfTask(TaskName, Hours);
        }

        public void AddResourceToTask(string ResourceName, string TaskName)
        {
            if (Resources.CheckIfWorkResourceExists(ResourceName) || Resources.CheckIfMaterialResourceExists(ResourceName))
            {
                ProjectTree.AddResourceToTask(ResourceName, TaskName);
            }
            else Print($"Resource {ResourceName} does not exist!\n");
        }

        public void AddCapacityToResourceOfTask(string ResourceName, string TaskName, int Capacity)
        {
            ProjectTree.AddCapacityToResourceOfTask(ResourceName, TaskName, Capacity);

            if(Resources.CheckIfWorkResourceExists(ResourceName))
            {
                if (Capacity <= Resources.WorkResourceList[ResourceName].AvailableCapacity) ProjectTree.AddCapacityToResourceOfTask(ResourceName, TaskName, Capacity);
                else Print($"Cannot add the new capacity {Capacity} because it is greater than the available capacity {Resources.WorkResourceList[ResourceName].AvailableCapacity}!\n");
            }
            else ProjectTree.AddCapacityToResourceOfTask(ResourceName, TaskName, Capacity);
        }

        public void DeleteResourceOfTask(string ResourceName, string TaskName)
        {
            ProjectTree.DeleteResourceOfTask(ResourceName, TaskName);
        }

        public void DeleteAllResourceOfTask(string TaskName)
        {
            ProjectTree.DeleteAllResourceOfTask(TaskName);
        }

        public void PrintInformationOfResourcesAndCostOfTask(string TaskName)
        {
            ProjectTree.PrintDetailedResourcesAndTotalCostInfoOfTask(TaskName, Resources);
        }

        public string GetTaskNameFromID(string TaskID)
        {
            return ProjectTree.GetTaskNameFromID(TaskID);
        }

        public string GetTaskIDFromTaskName(string TaskName)
        {
            return ProjectTree.GetTaskID(TaskName);
        }

        public void PrintInformationOfTask(string TaskName)
        {
            ProjectTree.PrintInfoOfTask(TaskName);
        }

        public void PrintDescriptionOfTask(string TaskName)
        {
            ProjectTree.PrintTaskDescription(TaskName);
        }

        public void AddWorkResource(string ResourceName)
        {
            Resources.AddNewWorkResource(ResourceName);
        }

        public void AddMaterialResource(string ResourceName)
        {
            Resources.AddNewMaterialResource(ResourceName);
        }

        public void ChangeNameOfWorkResource(string CurrentName, string NewName)
        {
            Resources.ChangeWorkResourceName(CurrentName, NewName);
        }

        public void ChangeNameOfMaterialResource(string CurrentName, string NewName)
        {
            Resources.ChangeMaterialResourceName(CurrentName, NewName);
        }

        public void DeleteWorkResource(string ResourceName)
        {
            Resources.DeleteWorkResource(ResourceName);

            foreach (KeyValuePair<string, string> Task in ProjectTree.TaskNameandIDDic)
            {
                DeleteResourceOfTask(ResourceName, Task.Key);
            }
        }

        public void DeleteMaterialResource(string ResourceName)
        {
            Resources.DeleteMaterialResource(ResourceName);

            foreach (KeyValuePair<string, string> Task in ProjectTree.TaskNameandIDDic)
            {
                DeleteResourceOfTask(ResourceName, Task.Key);
            }
        }

        public void SetStandardRateOfWorkResource(string ResourceName, float StandardRate)
        {
            Resources.SetStandardRateOfWorkResource(ResourceName, StandardRate);
        }

        public void SetStandardRateOfMaterialResource(string ResourceName, float StandardRate)
        {
            Resources.SetStandardRateOfMaterialResource(ResourceName, StandardRate);
        }

        public void SetOvertimeRateOfWorkResource(string ResourceName, float OvertimeRate)
        {
            Resources.SetOvertimeRateOfWorkResource(ResourceName, OvertimeRate);
        }

        public void SetCurrencyForAllResource(string Currency)
        {
            Resources.SetCurrencyForAllResources(Currency);
        }

        public void SetMaximumWorkingHoursPerDayOfWorkResource(string ResourceName, float Hours)
        {
            Resources.SetMaximumWorkingHoursPerDayOfWorkResource(ResourceName, Hours);
        }

        public void SetAvailableCapacityOfWorkResource(string ResourceName, int Capacity)
        {
            Resources.SetAvailableCapacityOfWorkResource(ResourceName, Capacity);
        }

        public void SetRateUnitOfMaterialResource(string ResourceName, string RateUnit)
        {
            Resources.SetRateUnitOfMaterialResource(ResourceName, RateUnit);
        }

        public void SetAccrueTypeOfWorkResource(string ResourceName, string AccrueType)
        {
            Resources.SetAccrueOfWorkResource(ResourceName, AccrueType);
        }

        public void PrintInformationOfWorkResource(string ResourceName)
        {
            Resources.PrintInfoOfWorkResource(ResourceName);
        }

        public void ChangeProjectName(string CurrentProjectName, string NewProjectName)
        {
            if (CurrentProjectName == ProjectTree.RootTask.TaskName && NewProjectName != "Start" && NewProjectName != "End") ProjectTree.ChangeTaskName(CurrentProjectName, NewProjectName);
            else if (NewProjectName == "Start" || NewProjectName == "End") Print("Cannot change name to Start or End!\n");
            else Print($"Project name {CurrentProjectName} not found!\n");
        }
        public void PrintInformationOfMaterialResource(string ResourceName)
        {
            Resources.PrintInfoOfMaterialResource(ResourceName);
        }

        private void Print(string text)
        {
            Console.Write(text);
        }

        public List<Task> GetTasks()
        {
            List<Task> tasks = new List<Task>();
            foreach (var taskPair in ProjectTree.TaskNameandIDDic)
            {
                Task? task = ProjectTree.FindTaskNode(taskPair.Key);
                if (task != null)
                {
                    tasks.Add(task);
                }
            }
            return tasks;
        }
    }
}