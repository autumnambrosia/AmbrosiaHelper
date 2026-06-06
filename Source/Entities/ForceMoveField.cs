using System.Data.Common;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AmbrosiaHelper.Entities;

[CustomEntity("AmbrosiaHelper/ForceMoveField")]
public class ForceMoveField : Entity {
    // Mystery enum...
    public enum Forcemoves {
        Left = -1,
        Right = 1,
        Revoke = 0,
        RevokeOnWalljump = -2
    }

    internal static bool playerIsWalljumping;

    private readonly Forcemoves mode;
    private readonly float fmTimer; // Forcemove timer

    private float gradientTimer;
    private float flashTimer;
    private readonly float flashTimerMax = 0.3f;
    private bool Audible;

    // Component thingie
    private Image icon;

    // Colors
    private readonly Color colorNormal;
    private readonly Color colorStart = Calc.HexToColor("#00ff00");
    private readonly Color colorEnd = Calc.HexToColor("#ff0000");

    public ForceMoveField(EntityData data, Vector2 offset)
        : base(data.Position + offset) {
        mode = (Forcemoves)data.Int("direction");
        fmTimer = data.Float("time");

        Visible = data.Bool("renders"); // No need to assign it to a variable first!
        Audible = data.Bool("audible");

        // Change the color of the entity depending on its type
        colorNormal = mode switch {
            Forcemoves.Revoke           => Calc.HexToColor("#e76a44"),
            Forcemoves.RevokeOnWalljump => Calc.HexToColor("#cf46eb"),
            _ => Calc.HexToColor("#33bbff")
        };

        Depth = Depths.Below;

        Collider = new Hitbox(data.Width, data.Height);
        Add(new PlayerCollider(OnStay));

        // https://cdn.discordapp.com/attachments/1259338672751247462/1289627935598317590/Screen_Recording_20240926_134512_CapCut.mp4
        icon = new Image(GFX.Game[$"objects/AmbrosiaHelper/forcemovefield/{mode}"]);
        icon.Position = new Vector2(data.Width / 2, data.Height / 2); // Center the icon within the field
        icon.CenterOrigin();
        Add(icon);
    }
    
    private void OnStay(Player player) {
        // Do nothing/exit early if we are still on cooldown
        if (gradientTimer > 0f) return;

        switch (mode) {
            // For revoke modes, gradientTimer is set directly to 0 and my current code doesn't detect it as a flash, so we invoke flash manually
            case Forcemoves.RevokeOnWalljump:
                // Only do it if, yknow, the player is walljumping
                if (playerIsWalljumping && player.forceMoveXTimer > 0) {
                    // RevokeOnWalljump revokes everything on walljump (:exploding_head:)
                    gradientTimer = player.forceMoveXTimer = player.forceMoveX = 0;
                    flashTimer = flashTimerMax;
                    Play("event:/AmbrosiaHelper/fmfield_revoke");
                    playerIsWalljumping = false; // Should you do it here?
                }
                break;
            case Forcemoves.Revoke:
                // Only revoke if you had forcemove to begin with to prevent effects triggering every frame
                if (player.forceMoveXTimer > 0) {
                    gradientTimer = player.forceMoveXTimer = 0;
                    flashTimer = flashTimerMax;
                    Play("event:/AmbrosiaHelper/fmfield_revoke");
                }
                break;
            default:
                player.forceMoveX = (int)mode;
                gradientTimer = player.forceMoveXTimer = fmTimer; // Also sets the timer for gradient visuals to the same thing
                Play("event:/AmbrosiaHelper/fmfield_start");
                break;
        }

    }

    public override void Update() {
        base.Update();

        // "Break" the field if forcemove gets overwritten by something else so it doesn't keep going anyway and look weird
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player is not null) {
            Logger.Debug(nameof(AmbrosiaHelperModule), 
                "forcemove=" + player.forceMoveXTimer.ToString() + "\ttimer=" + gradientTimer.ToString()
            );
            if (gradientTimer > 0f && player.forceMoveXTimer != gradientTimer) {
                gradientTimer = 0;
                flashTimer = flashTimerMax;
                Play("event:/AmbrosiaHelper/fmfield_revoke");
            }
        }

        // Update timer for white flash
        if (flashTimer > 0f) {
            flashTimer -= Engine.DeltaTime;
        }
        // Set flash timer if necessary
        if (gradientTimer < 0f) {
            gradientTimer = 0f;
            flashTimer = flashTimerMax;
            Play("event:/AmbrosiaHelper/fmfield_end");
        }
        // Update timer for color gradient
        if (gradientTimer > 0f) {
            gradientTimer -= Engine.DeltaTime;
        }
    }

	public override void Render() {
		base.Render();

        // Factor the flash color into the draw color that everything uses
        Color color = Color.Lerp(CalcDrawColor(), Color.White, Calc.Clamp(flashTimer / flashTimerMax, 0, 1));

        Draw.Rect(Collider, color * 0.7f);
        Draw.HollowRect(Collider, color);
        icon.Color = color;
	}

    private Color CalcDrawColor() {
        if (gradientTimer <= 0f) return colorNormal;
        return Color.Lerp(colorStart, colorEnd, 1 - (gradientTimer / fmTimer));
    }

    // Doing this as an easy way to make the entity inaudible
    private void Play(string e) {
        if (Audible) Audio.Play(e);
    }

    #region Hooks

    internal static void Load() {
        On.Celeste.Player.WallJump += modWallJump;
    }

    internal static void Unload() {
        On.Celeste.Player.WallJump -= modWallJump;
    }

    internal static void modWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir) {
        playerIsWalljumping = true;
        orig(self, dir);
    }

    #endregion
}