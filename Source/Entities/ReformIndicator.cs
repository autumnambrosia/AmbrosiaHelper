using System;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AmbrosiaHelper.Entities;

[CustomEntity("AmbrosiaHelper/ReformIndicator")]
public class ReformIndicator : Entity {
    private BounceBlock block;
    private float drawX;
    private float drawY;
    //private MoveBlock? moveblock;

    private List<Image> hotImages = [];
    private List<Image> coldImages = [];

    private bool spawnedByModSetting = false;

    public ReformIndicator(EntityData data, Vector2 offset) : base(data.Position + offset) {
        Depth = Depths.Solids + 10;
        Collider = new Hitbox(16,16,-8,-8);
        Add(new CoreModeListener(ToggleSprite));
    }

    public override void Awake(Scene scene) {
        base.Awake(scene);

        if (!spawnedByModSetting && AmbrosiaHelperModule.Settings.IndicateAllCoreBlocks) RemoveSelf(); // prevents duplicates w/ already placed indicator + mod setting enabled

        // pray that this shit is tracked
        //block = CollideFirst<BounceBlock>();
        // fuck
        // fucking evil shit
        block = (BounceBlock)Collide.First(this, Scene.Entities.FindAll<BounceBlock>());
        if (block is null) {RemoveSelf(); return;}

        // maybe do some thing with storing block x/y/w/h in fields
        drawX = block.X;
        drawY = block.Y;
        Position = block.Position;
        //Collider = block.Collider;

        // i have to do this twice and add a coremodelistener to make the outline line up because the sprites for hot and cold mode are different
        foreach (Image i in block.hotImages) {
            Rectangle cr = i.Texture.ClipRect;
            Image image = new Image(
                GFX.Game["objects/AmbrosiaHelper/core_outline"].GetSubtexture(cr.X-582, cr.Y-2589, 8, 8) // arbitrary fucking numbers but it works??
            ){
                Position = i.Position,
                Visible = !block.iceMode
            };
            hotImages.Add(image);
            Add(image);
        }
        foreach (Image i in block.coldImages) {
            Rectangle cr = i.Texture.ClipRect;
            //Console.WriteLine($"{cr.X},{cr.Y}");
            Image image = new Image(
                GFX.Game["objects/AmbrosiaHelper/core_outline"].GetSubtexture(cr.X-821, cr.Y-2138, 8, 8)
            ){
                Position = i.Position,
                Visible = block.iceMode
            };
            coldImages.Add(image);
            Add(image);
        }
    }

    private void ToggleSprite(Session.CoreModes mode) {
        foreach (Image hotImage in hotImages) {
			hotImage.Visible = mode != Session.CoreModes.Cold;
		}
		foreach (Image coldImage in coldImages) {
			coldImage.Visible = mode == Session.CoreModes.Cold;
		}
    }

    public override void Render() {
        base.Render();
        if (block is null) return;

        float progress = Calc.ClampedMap(block.respawnTimer,
            0f, 1.6f,
            1f, 0f
        );
        
        // d_ as "draw _"
        float dx = drawX + block.Width/8 + 1;
        float dy = drawY + (block.Height-6) / 2;
        float dw = (block.Width*3/4) - 2; // block.Width - (block.Width/4 + 2)

        Draw.HollowRect(
            x: dx,
            y: dy,
            width: dw,
            height: 6,
            Color.White
        );
        Draw.Rect(
            x: dx,
            y: dy,
            width: dw * progress,
            height: 6,
            Color.White
        );
    }

    // stuff for the indicate all core blocks modsetting

    public static void Load()   => Everest.Events.Level.OnLoadEntity += OnLoadEntity;
    public static void Unload() => Everest.Events.Level.OnLoadEntity -= OnLoadEntity;

    public static bool OnLoadEntity(Level level, LevelData levelData, Vector2 offset, EntityData entityData) {
        //|| entityData.Name == "AmbrosiaHelper/ChillingCoreBlock")
        if (AmbrosiaHelperModule.Settings.IndicateAllCoreBlocks && entityData.Name == "bounceBlock") {
            level.Add(new ReformIndicator(new EntityData() {
                Name = "AmbrosiaHelper/ReformIndicator",
                Position = entityData.Position,
                Values = []
            }, offset){
                spawnedByModSetting = true
            });
        }
        return false;
    }
}