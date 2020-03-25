using UnityEngine;

public class FightTutorial : MonoBehaviour
{
    public InfoBoard infoBoard;
    public InfoBoard.InfoBoardData[] messages;
    public Target[] targets;
    public int targetScore;
    public Interactable interactableTarget;
    
    private StoryPlayer player;
    private int completed;
    private int score;

    void Start()
    {
        infoBoard.StartDisplay(messages[0]);
        player = Player.Instance as StoryPlayer;
        player.ShotBlock = false;
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        player.GetComponent<Rigidbody2D>().gravityScale = 0;
        player.GetComponent<Rigidbody2D>().mass = 1000;
        foreach (var i in targets)
        {
            i.IsOnGround = false;
            i.onHit = new UnityEngine.Events.UnityAction(() => ScorePlus());
            i.ariseTime = 5f;
        }
    }

    private void ScorePlus()
    {
        score++;
    }

    private void Update()
    {
        if (completed < 1 && score >= targetScore)
        {
            completed = 1;
            infoBoard.StartDisplay(messages[1]);
            player.ReloadBlock = false;
        }
        if (completed == 1 && player.R && (player.weapon as Gun).Load < (player.weapon as Gun).magazineCapacity)
        {
            completed = 2;
            infoBoard.StartDisplay(messages[2]);
            player.GrenadeBlock = false;
        }
        if (completed == 2 && player.F && !player.weapon.IsReloading && player.thrower.grenade != null)
        {
            completed = 3;
            infoBoard.StartDisplay(messages[3]);
            player.TeleportBlock = false;
        }
        if (completed == 3 && player.Space)
        {
            completed = 4;
            infoBoard.StartDisplay(messages[4]);
            interactableTarget.IsLocked = false;
            interactableTarget.Interacted = false;
        }
        if (completed == 4 && interactableTarget.Interacted)
        {
            player.GrenadeBlock = true;
            player.TeleportBlock = true;
            player.ShotBlock = true;
            infoBoard.StopDisplay();
            completed = 5;
            (Controller.Instance as Controller00).Locked = false;
            var grip = GameObject.FindGameObjectWithTag("UIgrip").transform;
            for (var i = 0; i < grip.childCount; i++)
            {
                Destroy(grip.GetChild(i).gameObject, Random.Range(0f, 3f));
            }
            foreach (var i in targets)
            {
                i.IsOnGround = true;
                i.ariseTime = float.PositiveInfinity;
            }
        }
    }
}
