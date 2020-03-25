using UnityEngine;

public class WalkTutorial : MonoBehaviour
{
    public InfoBoard infoBoard;
    public float targetDistance;
    public InfoBoard.InfoBoardData[] messages;
    public Interactable interactionTarget;

    private Vector2 startPoint;
    private Player player;
    private int completed;

    void Start()
    {
        infoBoard.StartDisplay(messages[0]);
        player = Player.Instance;
        startPoint = player.transform.position;
    }

    private void Update()
    {
        if (completed < 1 && Vector2.Distance(startPoint, player.transform.position) >= targetDistance)
        {
            completed = 1;
            infoBoard.StartDisplay(messages[1]);
            interactionTarget.IsLocked = false;
        }
        if (completed < 2 && interactionTarget.Interacted)
        {
            completed = 2;
            infoBoard.StopDisplay();
            Destroy(gameObject);
        }
    }
}
