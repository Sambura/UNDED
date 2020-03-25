using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Controller00 : Controller
{
    private int step = 0;
    private bool _locked;
    public bool enableSkip = true;

    public bool Locked
    {
        get
        {
            return _locked;
        }
        set
        {
            if (value == false && _locked == true)
                step++;
            _locked = value;
        }
    }
    private float time;

    public List<CharacterMoveEvent> moveEvents;
    public List<ConversationEvent> conversationEvents;
    public List<InteractionEvent> interactionEvents;
    public List<ActionEvent> actionEvents;

    private Dictionary<int, Event> events;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(this);
            return;
        }
    }

    private void Start()
    {
        events = new Dictionary<int, Event>();
        var sorted = new SortedDictionary<float, Event>();
        foreach (var i in moveEvents)
        {
            i.eventType = 0;
            sorted.Add(i.eventIndex, i);
        }
        foreach (var i in conversationEvents)
        {
            i.eventType = 1;
            sorted.Add(i.eventIndex, i);
        }
        foreach (var i in interactionEvents)
        {
            i.eventType = 4;
            sorted.Add(i.eventIndex, i);
        }
        foreach (var i in actionEvents)
        {
            i.eventType = 5;
            sorted.Add(i.eventIndex, i);
        }

        int s = 0;
        foreach (var i in sorted)
        {
            events.Add(s, i.Value);
            s++;
        }
    }

    void Update()
    {
        /*
        if (Input.GetKey(KeyCode.Alpha1) && enableSkip)
        {
            Debug.Log("Skip pending...");
            time += 0.5f;
            if (events.ContainsKey(step))
            {
                var _event = events[step];
                switch (_event.eventType)
                {
                    case 0:
                        {
                            CharacterMoveEvent @event = _event as CharacterMoveEvent;
                            Character character = @event.character.GetComponent<Character>();
                            if (character.ignoreY)
                            {
                                character.transform.position = new Vector2(@event.point.position.x, character.transform.position.y);
                            } else
                            {
                                character.transform.position =  @event.point.position;
                            }
                            step++;
                            time = 0;
                            break;
                        }
                    case 1:
                        {
                            step++;
                            time = 0;
                            break;
                        }
                }     
            } 
            return;
        }*/

        if (Input.GetKeyDown(KeyCode.Alpha1) && enableSkip)
            if (Locked) Locked = false;

        if (!Locked)
        {
            time += Time.deltaTime;
            if (events.ContainsKey(step))
            {
                Event _event = events[step];
                if (_event.delay > time) return;
                switch (_event.eventType)
                {
                    case 0:
                        {
                            CharacterMoveEvent @event = _event as CharacterMoveEvent;
                            Character character = @event.character.GetComponent<Character>();
                            character.targetPoint = @event.point;
                            character.StartWalking(@event.duration);
                            break;
                        }
                    case 1:
                        {
                            ConversationEvent @event = _event as ConversationEvent;
                            DialogueManager.Instance.StartConversation(@event.dialogueIndex);
                            break;
                        }
                    case 4:
                        {
                            InteractionEvent @event = _event as InteractionEvent;
                            if (@event.unlockTarget)
                            {
                                @event.target.IsLocked = false;
                            }
                            if (@event.lockTarget)
                            {
                                @event.target.IsLocked = true;
                            }
                            @event.target.Interacted = false;

                            break;
                        }
                    case 5:
                        {
                            ActionEvent @event = _event as ActionEvent;
                            @event.action.Invoke();
                            break;
                        }
                }
                Locked = _event.lockStep;
                if (!Locked) step++;
                time = 0;
            }
        }
        else
        {
            Event _event = events[step];
            switch (_event.eventType)
            {
                case 0:
                    {
                        CharacterMoveEvent @event = _event as CharacterMoveEvent;
                        Character character = @event.character.GetComponent<Character>();
                        switch (@event.unlocker)
                        {
                            case Unlocker.onCompleted:
                                if (character.targetReached)
                                {
                                    Locked = false;
                                }
                                break;
                        }
                        break;
                    }
                case 1:
                    {
                        switch (_event.unlocker)
                        {
                            case Unlocker.onCompleted:
                                if (!DialogueManager.Instance.dialogueInProcess)
                                {
                                    Locked = false;
                                }
                                break;
                        }
                        break;
                    }
                case 4:
                    {
                        switch (_event.unlocker)
                        {
                            case Unlocker.onCompleted:
                                InteractionEvent @event = _event as InteractionEvent;
                                if (@event.target.Interacted)
                                    Locked = false;
                                break;
                        }
                        break;
                    }
            }
        }
    }

    public void LoadNextLevel()
    {
        //StartCoroutine(LoadNextLevelAsync());
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        Instance = null;
    }

    //private IEnumerator LoadNextLevelAsync()
    //{
     //   var process = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        //yield return new WaitWhile(() => process.progress < 0.9f);

    //}

    [System.Serializable]
    public class Event
    {
        public float delay;
        public bool lockStep;
        public Unlocker unlocker;
        [HideInInspector] public int eventType;
        public float eventIndex;
    }

    [System.Serializable]
    public class CharacterMoveEvent : Event
    {
        public GameObject character;
        public Transform point;
        public float duration;
    }

    [System.Serializable]
    public class ConversationEvent : Event
    {
        public int dialogueIndex;
    }

    [System.Serializable]
    public class InteractionEvent : Event
    {
        public Interactable target;
        public bool unlockTarget;
        public bool lockTarget;
    }

    [System.Serializable]
    public class Action : UnityEvent { }

    [System.Serializable]
    public class ActionEvent : Event
    {
        public Action action = new Action();
    }

    [System.Serializable]
    public enum Unlocker { anyKey, onCompleted, external };
}