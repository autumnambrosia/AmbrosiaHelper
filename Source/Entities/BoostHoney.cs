using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;

namespace Celeste.Mod.AmbrosiaHelper.Entities;

[CustomEntity("AmbrosiaHelper/BoostHoney")]
[Tracked]
public class BoostHoney : Entity {
    public enum Directions {
        Up = 0,
    	Left = -1,
    	Right = 1
    }

    // why do you have 3 fields that are just directions what is wrong with you
    private Directions boostDir;
    private readonly Directions placementDir;
    private readonly Directions fallbackDir;

    private readonly bool useCoreMode;

    private Vector2 speedVector;

    private LavaRect lavarect; // Component!

    // goodbye whimsical dictionary... o7
    /*
    private Dictionary<Session.CoreModes, Color[]> colorLookup = new Dictionary<Session.CoreModes, Color[]> {
        {Session.CoreModes.Hot,  RisingLava.Hot},
        {Session.CoreModes.Cold, RisingLava.Cold},
        {Session.CoreModes.None, [Calc.HexToColor("fcff33"), Calc.HexToColor("f2d729"), Calc.HexToColor("d19900")]}
    };
    */

    public BoostHoney(EntityData data, Vector2 offset)
        : base(data.Position + offset) {
        useCoreMode = data.Bool("useCoreMode");
        fallbackDir = data.Enum<Directions>("fallbackDir");
        placementDir = data.Enum<Directions>("placementDir");

        Depth = Depths.Below;
        Collider = placementDir switch {
            Directions.Up    => new Hitbox(data.Width, 4f, 0f, -4f),
            Directions.Left  => new Hitbox(4f, data.Height, -4f),
            Directions.Right => new Hitbox(4f, data.Height),
            _ => new Hitbox(data.Width, data.Height)
        };

        if (useCoreMode) Add(new CoreModeListener(OnCoreChange));
        //Add(new PlayerCollider(OnStay));
        Add(new StaticMover {
            OnShake = StaticMover_OnShake,
            SolidChecker = StaticMover_IsRiding,
            JumpThruChecker = StaticMover_IsRiding,
            OnEnable  = () => Active = Visible = Collidable = true,
            OnDisable = () => Active = Visible = Collidable = false
        });

        Add(lavarect = new LavaRect(Collider.Width, Collider.Height, 4));
        lavarect.Position = Collider.Position; // Make it go in the right spot since we're changing stuff based off direction

        // gonna be honest i just put like random numbers into this one
        lavarect.SmallWaveAmplitude = 0.5f;
        lavarect.BigWaveAmplitude = 0.75f;
        lavarect.CurveAmplitude = 0f;

        // generating the arrow tiles
        // math time
        int length = (int)(placementDir == Directions.Up ? Width : Height);
        Vector2 offsetUnit = placementDir == Directions.Up ? Vector2.UnitX : Vector2.UnitY;
        int startPos = (length % 8) / 2;
        for (int i = 0; i < length; i += 8) {
            Image tile = new Image(GFX.Game[$"objects/AmbrosiaHelper/boosthoney/{(int)boostDir}"]);
            tile.Position = Collider.Position + (offsetUnit * (i + startPos)) + DetermineTileJustification(boostDir);
            Add(tile);
        }
    }

	public override void Added(Scene scene) {
		base.Added(scene);

        // I had the code copied twice here and there but i could literally just call the method here
        Session.CoreModes mode = useCoreMode ? SceneAs<Level>().CoreMode : Session.CoreModes.None;
        OnCoreChange(mode);
    }

    /*
    private void OnStay(Player player) {
        player.LiftSpeed += speedVector;
    }
    */

    private Directions DetermineBoostDirection(Session.CoreModes mode) {
        if (mode == Session.CoreModes.None || !useCoreMode) return fallbackDir;
        if (mode == Session.CoreModes.Hot) return Directions.Up;
        // past this point core mode is cold
        if (placementDir != Directions.Up) return placementDir;
        if ( fallbackDir != Directions.Up) return fallbackDir;
        return Directions.Right;
    }

    // See if you need to balance these values to make gp better
    private Vector2 DetermineSpeedVector() => boostDir switch {
        Directions.Up    => new Vector2(0f, -170f),
        Directions.Left  => new Vector2(-85f, 0f),
        Directions.Right => new Vector2( 85f, 0f),
        _ => Vector2.Zero
    };

    private Vector2 DetermineTileJustification(Directions x) => (placementDir, x) switch {
        (Directions.Up,    Directions.Up) =>     Vector2.Zero,
        (Directions.Up,    _            ) => new Vector2(1, -3),
        (Directions.Left,  Directions.Up) => new Vector2(-2, 2),
        (Directions.Right, Directions.Up) => new Vector2(0, 2),
        (_, _) => Vector2.Zero
    };

    #region Components

    private void OnCoreChange(Session.CoreModes mode) {
        Color[] colors = mode switch {
            Session.CoreModes.Hot  => RisingLava.Hot,
            Session.CoreModes.Cold => RisingLava.Cold,
            _ => [
                Calc.HexToColor("#fcff33"),
                Calc.HexToColor("#f2d729"),
                Calc.HexToColor("#d19900")
            ]
        };

        lavarect.SurfaceColor = colors[1];
        lavarect.EdgeColor = colors[1];
        lavarect.CenterColor = colors[2];

        Directions newdir = DetermineBoostDirection(mode);
        foreach (Component component in Components) {
            if (component is Image tile) {
                tile.Texture = GFX.Game[$"objects/AmbrosiaHelper/boosthoney/{(int)newdir}"];
                tile.Color = colors[0];

                // This SUCKS
                tile.Position -= DetermineTileJustification(boostDir);
                tile.Position += DetermineTileJustification(newdir);
            }
        }

        boostDir = newdir;
        speedVector = DetermineSpeedVector();
    }

    private void StaticMover_OnShake(Vector2 shake) {
        // TODO this felt like kind of janky so see if you can maybe fix it but it's fine if you dont because it's like fine anyway
        // lavarect.Position = Collider.Position + shake;
        lavarect.Position = Collider.Position;
    }

    private bool StaticMover_IsRiding(Solid solid) => placementDir switch {
        Directions.Up    => CollideCheckOutside(solid, Position + Vector2.UnitY), 
        Directions.Left  => CollideCheckOutside(solid, Position + Vector2.UnitX), 
        Directions.Right => CollideCheckOutside(solid, Position - Vector2.UnitX), 
        _ => false
    };

    private bool StaticMover_IsRiding(JumpThru jumpThru) {
        if (placementDir != Directions.Up) return false;
        return CollideCheck(jumpThru, Position + Vector2.UnitY);
    }

    #endregion

    #region Hooks

    internal static void Load() {
        On.Celeste.Player.Jump += Honey_Jump;
        On.Celeste.Player.WallJump += Honey_WallJump;
        //On.Celeste.Player.ClimbJump += Honey_ClimbJump;
        On.Celeste.Player.SuperJump += Honey_SuperJump;
        On.Celeste.Player.SuperWallJump += Honey_SuperWallJump;

        // Literally just for extra audio effects
        // 
        On.Celeste.Player.ClimbBegin += Honey_ClimbBegin;
        //IL.Celeste.Player.OnCollideV += Honey_OnCollideV;
    }

    internal static void Unload() {
        On.Celeste.Player.Jump -= Honey_Jump;
        On.Celeste.Player.WallJump -= Honey_WallJump;
        //On.Celeste.Player.ClimbJump -= Honey_ClimbJump;
        On.Celeste.Player.SuperJump -= Honey_SuperJump;
        On.Celeste.Player.SuperWallJump -= Honey_SuperWallJump;

        On.Celeste.Player.ClimbBegin -= Honey_ClimbBegin;
        //IL.Celeste.Player.OnCollideV -= Honey_OnCollideV;
    }

    internal static void FuckassHookThing(Player self) {
        foreach (BoostHoney entity in self.Scene.Tracker.GetEntities<BoostHoney>()) {
            if (self.CollideCheck(entity)) {
                self.Speed += entity.speedVector;
                Audio.Play("event:/AmbrosiaHelper/honey_jump", entity.Center);
                Audio.Play("event:/char/madeline/jump_super", self.Center);
                self.launched = true;
            }
        }
    }

    // Dude like literally every hook is the same there's gotta be a better way to do this
    internal static void Honey_Jump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx) {
        orig(self, particles, playSfx);
        FuckassHookThing(self);
    }

    internal static void Honey_WallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
        orig(self, dir);
        FuckassHookThing(self);
    }

    internal static void Honey_SuperJump(On.Celeste.Player.orig_SuperJump orig, Player self) {
        orig(self);
        FuckassHookThing(self);
    }

    internal static void Honey_SuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir) {
        orig(self, dir);
        FuckassHookThing(self);
    }

    internal static void FuckassHookThing2(Player self) {
        foreach (BoostHoney entity in self.Scene.Tracker.GetEntities<BoostHoney>()) {
            if (self.CollideCheck(entity)) {
                Audio.Play("event:/AmbrosiaHelper/honey_enter", entity.Center);
            }
        }
    }

    internal static void Honey_ClimbBegin(On.Celeste.Player.orig_ClimbBegin orig, Player self) {
        orig(self);
        FuckassHookThing2(self);
    }

    // half-written il hook i gave up on because it was going to be for
    // literally a single sound effect and i decided it wasn't worth it
    /*
    internal static void Honey_OnCollideV(ILContext il) {
        ILCursor cursor = new ILCursor(il);

        if (!cursor.TryGotoNextBestFit(MoveType.After, 
            instr => instr.MatchConvR4(),
            instr => instr.MatchCallvirt<Player>("Play")
        )) {
            return;
        }

        cursor.EmitDelegate(FuckassHookThing3);
        cursor.EmitBrfalse();
        cursor.EmitCallvirt();
    }

    internal static bool FuckassHookThing3() {
        Player player = Scene.Tracker.GetEntity<Player>();
        if (CollideCheck<Player>()) {
            return true;
        }
        return false;
    }
    */

    #endregion
}