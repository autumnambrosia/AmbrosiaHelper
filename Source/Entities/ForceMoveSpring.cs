using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AmbrosiaHelper.Entities;

[CustomEntity("AmbrosiaHelper/ForceMoveSpring", "AmbrosiaHelper/ForceMoveSpringLeft", "AmbrosiaHelper/ForceMoveSpringRight")]
public class ForceMoveSpring : Spring {
    public ForceMoveSpring(EntityData data, Vector2 offset)
        : base(data.Position + offset, generateOrientation(data.Name), data.Bool("playerCanUse", true)) {
        
    }  
    
    private static Orientations generateOrientation(string name) {
        switch (name) {
            case "AmbrosiaHelper/ForceMoveSpring":
                return Orientations.Floor;
            case "AmbrosiaHelper/ForceMoveSpringRight":
                return Orientations.WallRight;
            case "AmbrosiaHelper/ForceMoveSpringLeft":
                return Orientations.WallLeft;
            default:
                return Orientations.Floor;
        }
    }
}