using System;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod;

namespace Celeste.Mod.AmbrosiaHelper.Entities;

[CustomEntity("AmbrosiaHelper/ChillingCoreBlock")]
public class ChillingCoreBlock : BounceBlock {
    public ChillingCoreBlock(EntityData data, Vector2 offset) : base(data, offset) {
        // Welcome to Arctic radio where we play nothing but cold, ice, and freezing. this aint yo ovens station
        Remove(Get<CoreModeListener>());
        iceMode = iceModeNext = true;

        // Remove all of BounceBlock's Images so that we can build our own. foreach doesn't work because we're removing the Images as we iterate
        for (int c = 0; c < Components.Count; c++) {
            if (Components[c] is Image image) Remove(image);
        }
        coldImages = BuildSprite(GFX.Game["objects/AmbrosiaHelper/chillingcoreblock/ice00"]);

        // Remove the center sprite then add a new one instead of using CreateOn b/c component ordering so it renders in front of the block
        Remove(coldCenterSprite);
        coldCenterSprite = GFX.SpriteBank.Create("AmbrosiaHelper_chillingsnowflake");
        coldCenterSprite.Position = new Vector2(Width, Height) / 2f;
        Add(coldCenterSprite);
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Added(Monocle.Scene)")]
    public extern void Entity_Added(Scene scene);

    // Bypass the setting of iceModeNext. would cause some really weird problems if loaded in hot mode
    public override void Added(Scene scene) {
        Entity_Added(scene);
        ToggleSprite();
    }

    [MonoModLinkTo("Monocle.Entity", "System.Void Update()")]
    public extern void Entity_Update();

    public override void Update() {
        Entity_Update();
        reappearFlash = Calc.Approach(reappearFlash, 0f, Engine.DeltaTime * 8f);

        switch (state) {
            case States.Waiting:
                Player player = WindUpPlayerCheck();
                if (player != null) {
                    moveSpeed = 80f;
                    // windUpStartTimer = 0f;
                    // bounceDir = -Vector2.UnitY;
                    state = States.WindingUp;

                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                    // StartShaking(0.2f);
                    Audio.Play("event:/AmbrosiaHelper/chillingblock_touch", Center);
                }
                break;
            case States.WindingUp:
				// i imagine a large portion of this is meant for hot core blocks and i dont actually need most of it but i'm kind of scared to really change it
                moveSpeed = Calc.Approach(moveSpeed, 35f, 600f * Engine.DeltaTime);
                Vector2 targetPos = startPos + (Vector2.UnitY * 16f);
                Vector2 nextPos = Calc.Approach(ExactPosition, targetPos, moveSpeed * Engine.DeltaTime / 3f);
                Vector2 liftSpeed = (nextPos - ExactPosition).SafeNormalize(moveSpeed / 3f);
                MoveTo(nextPos, liftSpeed);

                /*
                if (Vector2.DistanceSquared(ExactPosition, targetpos) <= 12f) {
                    StartShaking(0.1f);
                }
                */
                if (Vector2.DistanceSquared(ExactPosition, targetPos) <= 2f) {
                    Break(); // i know there are debris sprites i should probably change in code but i cba rn
                    Celeste.Freeze(0.05f); // currently 3f. no more than 4f or buffering doesn't work
                    moveSpeed = 0f;
                }
                break;
            case States.Broken:
                Depth = 8990;
                reformed = false;
                if (respawnTimer > 0f) {
                    respawnTimer -= Engine.DeltaTime;
                    return;
                }
                
                // Here, respawn timer is over

                Vector2 position = Position;
                Position = startPos;
                // See if we're allowed to reform
                if (!CollideCheck<Actor>() && !CollideCheck<Solid>()) {
                    // CheckModeChange();
                    Audio.Play("event:/AmbrosiaHelper/chillingblock_reappear", Center);

                    float duration = 0.35f;
                    for (int i = 0; i < Width; i += 8) {
                        for (int j = 0; j < Height; j += 8) {
                            Vector2 debrisvector = new Vector2(X + i + 4f, Y + j + 4f);
                            Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(debrisvector + (debrisvector - Center).SafeNormalize() * 12f, debrisvector, true, duration));
                        }
                    }

                    Alarm.Set(this, duration, () => {
                        reformed = true;
                        reappearFlash = 0.6f;
                        EnableStaticMovers();
                        ReformParticles();
                    });

                    Depth = -9000;
                    MoveStaticMovers(Position - position);
                    Collidable = true;
                    state = States.Waiting;
                    Celeste.Freeze(0.05f); // currently 3f. no more than 4f or buffering doesn't work
                } else {
                    Position = position;
                }
                break;
        }
    }
}