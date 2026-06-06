using System.Data.Common;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AmbrosiaHelper.Entities;

[CustomEntity("AmbrosiaHelper/FlagOnForceMoveController")]
public class FlagOnForceMoveController : Entity {

    private readonly int direction;
    private readonly string flagname;
    private bool flagvalue;

    public FlagOnForceMoveController(EntityData data, Vector2 offset) {
        direction = data.Int("direction");
        flagname = data.Attr("flag");
    }

    public override void Update() {
        base.Update();

        Player player = Scene.Tracker.GetEntity<Player>();
        if (Scene is Level level && player is not null) {
            flagvalue = player.forceMoveXTimer > 0;

            // Add an extra constraint if a direction is set
            if (direction != 0) {
                flagvalue = flagvalue && (player.forceMoveX == direction);
            }

            level.Session.SetFlag(flagname, flagvalue);
        }
    }
}