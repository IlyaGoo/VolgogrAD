using UnityEngine;

/** Класс отдельного шаблона таска, по которому инициализируем TaskController **/
public class TaskInfo: TimeDoing {
    public GameObject taskObject;
    public string newTaskName;
    public TaskType type;
    public bool needButtons;
    public bool needShowMenu;
    public TaskControllerScript currentScript;
    
    public bool needStartAfterCreate;
    
    public TaskInfo(GameObject taskObject, string newTaskName, TaskType type, bool needButtons, bool needShowMenu, int startTime, int endTime, bool needStartAfterCreate)
        :base(startTime, endTime)
    {
        this.taskObject = taskObject;
        this.newTaskName = newTaskName;
        this.type = type;
        this.needButtons = needButtons;
        this.needShowMenu = needShowMenu;
        this.needStartAfterCreate = needStartAfterCreate;
    }
}