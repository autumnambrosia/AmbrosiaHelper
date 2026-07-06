using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.CompilerServices;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.AmbrosiaHelper.Entities;

// metal bumper? i hardly know h- *gets whacked with a metal bumper*
[CustomEntity("AmbrosiaHelper/MetalBumper")]
public class MetalBumper : Bumper {

    private bool isStatic;
    private Session.CoreModes coreOverride;

    // Unlobotomizing my bumper. No fieldslop.
    /*
    private float launchSpeed;
    private string hitsfxName;
    private string respawnsfxName;
    private string spriteName;
    private List<Color> coldparticleColors = new(); // adding support for custom particle colors singlehandedly increased this code by like 3 million lines
    private List<Color> hotparticleColors = new();
    private float respawnSeconds;
    private int angleZones;
    */

    public MetalBumper(EntityData data, Vector2 offset)
        : base(data, offset) {
        isStatic = data.Bool("static", true);
        coreOverride = data.Enum<Session.CoreModes>("coremode");

        // I did. Whatchu gonna do about it?
        /*
        hitsfxName = data.Attr("sfxname", "event:/game/09_core/pinballbumper_hit");
        respawnsfxName = data.Attr("respawnsfxname", "event:/game/06_reflection/pinballbumper_reset");
        spriteName = data.Attr("spritename", "AmbrosiaHelper_metalbumper");
        launchSpeed = data.Float("speed", 300f);
        
        respawnSeconds = data.Float("respawntime", 0.5f);
        angleZones = data.Int("anglezones", 8);

        // Evil particles time (part 1)
        string[] coldcolors = data.Attr("coldparticles", "47b5cc,c4f4ff").Split(",");
        foreach (string c in coldcolors) coldparticleColors.Add(Calc.HexToColor(c));

        string[] hotcolors = data.Attr("hotparticles", "ffa808,ffa808").Split(",");
        foreach (string c in hotcolors) hotparticleColors.Add(Calc.HexToColor(c));
        */

        sprite     = GFX.SpriteBank.CreateOn(sprite,     "AmbrosiaHelper_metalbumper");
        spriteEvil = GFX.SpriteBank.CreateOn(spriteEvil, "AmbrosiaHelper_metalbumper_evil");

        // I love stealing code
        Get<PlayerCollider>().OnCollide = NEW_OnPlayer;

        // We want hitting the bumper to not kill us in fire mode; we don't need this
        // (removing the component that gets inherited from regular Bumper)
        Remove(hitWiggler);

        if (isStatic) {
            // sine is from regular Bumper, we are removing the SineWave component.
            Remove(sine);
        }
    }

    public override void Added(Scene scene) {
        base.Added(scene);

        // This is placed after Added so that it can properly overwrite the core mode set in Added
        // None in this case means "abide by core mode".
        // Any other setting acts as an override and forces the bumper into one particular core mode
        if (coreOverride != Session.CoreModes.None) {
            Remove(Get<CoreModeListener>()); 
            OnChangeMode(coreOverride);
        }
    }

    /*
    // This is here so that we can call Entity's Update instead of Bumper's Update
    // Normally, we use base.Update() because base is Entity, but here base is actually Bumper, so we have to use other methods
    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    public extern void Entity_Update();

    // I am LITERALLY just doing this for Custom Particles And Sounds. Agh.
    public override void Update() {
		Entity_Update();

		if (respawnTimer > 0f) {
			respawnTimer -= Engine.DeltaTime;
			if (respawnTimer <= 0f) {
				light.Visible = bloom.Visible = true;
				sprite.Play("on");
				spriteEvil.Play("on");

				Audio.Play(respawnsfxName, Position); // respawn sound
			}
		} else if (Scene.OnInterval(0.05f)) {
			float randomangle = Calc.Random.NextAngle();

            // particle shit. part 2
            var particlelist = fireMode ? hotparticleColors : coldparticleColors;
            ParticleType particletype = new ParticleType(P_Ambience) {
                Color = particlelist[0],
                Color2 = particlelist[1]
            };

			SceneAs<Level>().Particles.Emit(particletype, 1, Center + Calc.AngleToVector(randomangle, 10), Vector2.One * 2f, randomangle);
		}

		UpdatePosition();
	}
    */

    // copied from vanilla bumper but i changed stuff
    // it was too new
    private void NEW_OnPlayer(Player player) {
        // The player can still collide with PlayerCollider even if it's respawning!
        // So: if it's still respawning, do nothing
        if (respawnTimer > 0f) return;

        respawnTimer = 0.5f;

        // # Explode launch
        Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
        Celeste.Freeze(0.1f);

        /*
        // Failsafe because otherwise the game will.. completely freeze and force you to close it
        if (angleZones == 0) throw new DivideByZeroException("This is a failsafe to prevent the game from freezing; do not set angle zones to zero.");
        */

        // math part 2
        float hitAngle = (player.Center - Center).Angle();
        float outputAngle = MathF.Round(hitAngle / (MathF.Tau/8f)) * MathF.Tau/8f;
        // the 8-zone effect is done with dividing by τ/8 -> rounding -> multiplying by τ/8 back
        // now with a custom field so that it can be any number (because everyone loves customization for some reason)

        Vector2 normalvector = new Vector2(MathF.Cos(outputAngle), MathF.Sin(outputAngle));
        if (fireMode) normalvector = -normalvector;
        player.Speed = normalvector * 300f;

        // Maybe code something for bumperboosting too.
        // I'm honestly not sure if i want to have it or not

        sprite    .Play("hit", restart: true);
        spriteEvil.Play("hit", restart: true);
        light.Visible = bloom.Visible = false;

        Audio.Play("event:/game/09_core/pinballbumper_hit", Position);

        // All the effects nd shi
        SlashFx.Burst(player.Center, outputAngle);
        if (!player.Inventory.NoRefills) player.RefillDash();
        player.RefillStamina();
        player.dashCooldownTimer = 0.2f;
        player.StateMachine.State = 7; // maybe make it not do this?

        SceneAs<Level>().DirectionalShake(normalvector, 0.15f);
        SceneAs<Level>().Displacement.AddBurst(Center, 0.3f, 8f, 32f, 0.8f);

        /*
        // particles part 3. it's the same as part 2 with slightly different fields
        var particlelist = fireMode ? hotparticleColors : coldparticleColors;
        ParticleType particletype = new ParticleType(P_Launch) {
            Color = particlelist[0],
            Color2 = particlelist[1]
        };
        */

        SceneAs<Level>().Particles.Emit(fireMode ? P_FireHit : P_Launch, 12, Center + normalvector * 12f, Vector2.One * 3f, normalvector.Angle());
    }

    // vert cameo in my codemod??
    // say hi to vert
    
    // hi vert
}