using UnityEngine;
using Mirror;
using UnityEngine;

/** Класс отдельного шаблона квеста, по которому спавним и инициализируем QuestController */
public class TaskInfo: TimeDoing {
    string ControllerName;//Имя префаба контроллера квеста
    public GameObject Controller;//todo вообще плохо, что мы храним конкретный объект, но пока допустимо
    public readonly string TaskName;
    public readonly TaskType NeedTaskType;
    public readonly bool NeedButtons;
    public readonly bool NeedShowMenu;
    /** Необходимо ли сразу спавнить миниигру (например, для сна, миниигра проверяет что все спят) */
    public readonly bool NeedStartAfterCreate;
    /** Некоторые квесты необходимо активировать в какой-то области (например, в готовку сначала вписаться нужно) */
    IStarQuestObjectGetter objectGetter;
    private GameObject QuestActivator;

    /** За сервер необходимо заспавнить префаб объекта квеста */
    public void StartQuest(){
        Controller = UnityEngine.Object.Instantiate(Resources.Load("Quests/Quest"), Vector3.zero, Quaternion.identity) as GameObject;
        Controller.GetComponent<QuestController>().Init(TaskName, NeedTaskType, 0);
        NetworkServer.Spawn(Controller);//После спавна контроллер сам запросит у сервера данные

        if (objectGetter != null) {
            var obj = objectGetter.GetObject();
            //Далее спавним зону триггера начала, устанавливая ему в парента необходимый объект, с которого он берет настройки коллайдера и позиции
            QuestActivator = UnityEngine.Object.Instantiate(Resources.Load("Quests/QuestActivator"), obj.transform.position, Quaternion.identity, obj.transform) as GameObject;
            QuestActivator.GetComponent<QuestStartAreaDoing>().SetQuest(Controller.GetComponent<QuestController>());//У сервера сразу ставим связь, чтобы клиенту было что запрашивать
            NetworkServer.Spawn(QuestActivator);
        }

        if (NeedStartAfterCreate) GameSystem.instance.localCommands.CmdStartQuest(Controller);
    }

    public void EndQuest(){//server
        Controller.GetComponent<QuestController>().TimeOut();
    }
    
    public TaskInfo(
        string controllerName, 
        string taskName, 
        TaskType type,
        bool needButtons, 
        bool needShowMenu, 
        int startTime, 
        int endTime,
        bool needStartAfterCreate,
        IStarQuestObjectGetter needObjectGetter = null
        ) :base(startTime, endTime)
    {
        objectGetter = needObjectGetter;
        ControllerName = controllerName;
        TaskName = taskName;
        NeedTaskType = type;
        NeedButtons = needButtons;
        NeedShowMenu = needShowMenu;
        NeedStartAfterCreate = needStartAfterCreate;
    }
}