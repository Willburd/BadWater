using Behaviors_BASE;
using Godot;
using GodotPlugins.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;


public partial class AbstractEntity
{
    // Beginning of template data
    protected PackRef PackRef;
    protected MapController.GridPos grid_pos;
    protected NetworkClient owner_client;
    public MapController.GridPos GridPos
    {
        get {return grid_pos;}
    }
    public virtual void ApplyMapCustomData(Godot.Collections.Dictionary data)
    {
        // Update our template with newly set variables
        PackData template_data = TemplateWrite();
        template_data.SetVars(data); // Override with custom set!
        TemplateRead(template_data);
        MapLoadVars(data);
    }
    public virtual void MapLoadVars(Godot.Collections.Dictionary data)
    {
        
    }
    public virtual void TemplateRead(PackData data)
    {
        PackRef = new PackRef( data, entity_type);
        SetTag(data.tag);
        display_name    = new DisplayName(data.display_name);
        description     = data.description;
        model           = data.model;
        texture         = data.texture;
        anim_speed      = data.anim_speed;
        attack_range    = data.attack_range;
        attack_force    = data.attack_force;
        embed_chance    = data.embed_chance;
        damtype         = data.damtype;
        intangible      = data.intangible;
        unstoppable     = data.unstoppable;
        hit_sound       = data.hit_sound;
    }
    public PackData TemplateWrite()
    {
        PackData data = null;
        switch(entity_type)
        {
            case MainController.DataType.Map:
                data = new MapData();
            break;
            case MainController.DataType.Area:
                data = new AreaData();
            break;
            case MainController.DataType.Turf:
                data = new TurfData();
            break;
            case MainController.DataType.Effect:
                data = new EffectData();
            break;
            case MainController.DataType.Item:
                data = new ItemData();
            break;
            case MainController.DataType.Structure:
                // data = new StructureData();
            break;
            case MainController.DataType.Machine:
                // data = new MachineData();
            break;
            case MainController.DataType.Mob:
                data = new MobData();
            break;
        }
        data.Clone(AssetLoader.GetPackFromRef(PackRef));
        return data;
    }
    public string tag = "";
    public string model = "BASE/Turf/Plane.tscn";
    public string texture = "";
    public double anim_speed = 0;

    public string icon_state = "";
    
    public DAT.Dir direction = DAT.Dir.South;
    public DisplayName display_name = new DisplayName("");
    public string description = "";
    public bool density = false;              // blocks movement
    public bool opaque = false;               // blocks vision
    public string step_sound = "";              // Sound pack ID for steps
    public string hit_sound = "";               // Weapon attack sound when used
    public int attack_range = 1;
    public float attack_force = 1f;
    public int embed_chance = 0;
    public DAT.DamageType damtype = DAT.DamageType.BRUTE;
    public bool intangible = false;           // can move through solids
    public bool unstoppable = false; 
    // End of template data

    // state data
    private float last_bump_time = 0;
    private const float bump_reset_time = 30; // Ticks
    public string GetUniqueID
    {
        get { return PackRef.modid; }
    }
    public virtual DAT.ZoneSelection SelectingZone
    {
        get {return DAT.ZoneSelection.UpperBody;}
    }
    private DAT.Intent internal_selecting_intent = DAT.Intent.Help;
    public DAT.Intent SelectingIntent
    {
        get {return internal_selecting_intent;}
    }
    public void IntentSwap()
    {
        if(internal_selecting_intent == DAT.Intent.Help)
        {
            internal_selecting_intent = DAT.Intent.Hurt;
        }
        else
        {
            internal_selecting_intent = DAT.Intent.Help;
        }
    }
    public virtual DAT.SizeCategory SizeCategory
    {
        get { return DAT.SizeCategory.MEDIUM; }
    }
    protected MainController.DataType entity_type;
    // end state data

    public static AbstractEntity CreateEntity( MainController.DataType type, string type_ID, string map_id, Vector3 pos,bool suppress_init = false)
    {
        MapController.GridPos? grid = new MapController.GridPos(map_id,pos);
        return CreateEntity( type, type_ID, grid, suppress_init);
    }
    public static AbstractEntity CreateEntity( MainController.DataType type, string type_ID, MapController.GridPos? pos,bool suppress_init = false, string data_string = "")
    {
        PackData typeData = null;
        AbstractEntity newEnt = null;
        switch(type)
        {
            case MainController.DataType.Area:
                typeData = AssetLoader.loaded_areas[type_ID];
                newEnt = new AbstractArea();
                newEnt.entity_type = type;
                break;
            case MainController.DataType.Turf:
                typeData = AssetLoader.loaded_turfs[type_ID];
                newEnt = AbstractTurf.CreateTurf(typeData, data_string);
                newEnt.entity_type = type;
                break;
            case MainController.DataType.Effect:
                typeData = AssetLoader.loaded_effects[type_ID];
                newEnt = AbstractEffect.CreateEffect(typeData, data_string);
                newEnt.entity_type = type;
                MapController.controller.effects.Add(newEnt as AbstractEffect);
                break;
            case MainController.DataType.Item:
                typeData = AssetLoader.loaded_items[type_ID];
                newEnt = AbstractItem.CreateItem(typeData, data_string);
                newEnt.entity_type = type;
                MapController.controller.entities.Add(newEnt);
                break;
            case MainController.DataType.Structure:
                typeData = AssetLoader.loaded_structures[type_ID];
                newEnt = AbstractStructure.CreateStructure(typeData, data_string);
                newEnt.entity_type = type;
                MapController.controller.entities.Add(newEnt);
                break;
            case MainController.DataType.Machine:
                typeData = AssetLoader.loaded_machines[type_ID];
                newEnt = AbstractMachine.CreateMachine(typeData, data_string);
                newEnt.entity_type = type;
                MachineController.controller.entities.Add(newEnt);
                break;
            case MainController.DataType.Mob:
                typeData = AssetLoader.loaded_mobs[type_ID];
                newEnt = AbstractMob.CreateMob(typeData, data_string);
                newEnt.entity_type = type;
                MobController.controller.entities.Add(newEnt);
                break;
        }
        // NetworkEntity init
        newEnt.grid_pos = new MapController.GridPos("NULL",0,0,0); // nullspace till placed
        newEnt.TemplateRead(typeData);
        // Automove to location
        if(pos != null) newEnt.Move(pos.Value,false);
        // Init, handles basic object spawning!
        if(!suppress_init)
        {
            newEnt.Init();
            newEnt.LateInit();
            newEnt.UpdateIcon();
        }
        return newEnt;
    }

    /*****************************************************************
     * Behavior hooks
     ****************************************************************/
    private NetworkEntity internal_loaded_network_entity;    // Puppet this
    public NetworkEntity LoadedNetworkEntity
    {
        get {return internal_loaded_network_entity;}
    }
    public virtual void Init() {} // Called upon creation to set variables or state, usually detected by map information.
    public virtual void LateInit() { } // Same as above, but when we NEED everything else Init() before we can properly tell our state!
    public virtual void Tick() { } // Called every process tick on the Fire() tick of the subcontroller that owns them
    public virtual void Crossed(AbstractEntity crosser) { }
    public virtual void UnCrossed(AbstractEntity crosser) { }
    public virtual void UpdateIcon() // Remember to call base.UpdateIcon() to handle transmitting data to the clients!
    { 
        // It's tradition~ Pushes graphical state changes.
        UpdateNetwork(true,false);
    } 
    public void Bump(AbstractEntity hitby) // When we are bumped by an incoming entity
    {
        if(MainController.WorldTicks <= last_bump_time + bump_reset_time) return;
        last_bump_time = MainController.WorldTicks;
    }
    public virtual void UpdateCustomNetworkData() { } // overload for custom data to be set on the network entity, rarely used
    
    /*****************************************************************
     * Processing
     ****************************************************************/
    public Vector3 velocity = Vector3.Zero;
    public void Process()
    {
        // Handle the tick!
        DAT.Dir old_dir = direction;
        Tick();
        ProcessVelocity();
        UpdateNetworkDirection(old_dir);
    }
    private void ProcessVelocity()
    {
        // Containers don't update velocity, and don't move...
        if(location != null) 
        {
            velocity *= 0;
            return;
        }
        // Update position if it has velocity
        if(velocity.Length() < 0.01) velocity *= 0;
        if(velocity != Vector3.Zero)
        {
            Move(new MapController.GridPos(grid_pos.GetMapID(),TOOLS.GridToPosWithOffset(grid_pos) + velocity));
        }
    }
    public void DeleteEntity()
    {
        switch(entity_type)
        {
            case MainController.DataType.Area:
                MapController.controller.areas.Remove(this.GetUniqueID);
                break;
            case MainController.DataType.Turf:
                break;
            case MainController.DataType.Effect:
                MapController.controller.effects.Remove(this as AbstractEffect);
                if((this as AbstractEffect).is_spawner) MapController.controller.spawners[(this as AbstractEffect).GetTag()].Remove(this as AbstractEffect);
                break;
            case MainController.DataType.Item:
                MapController.controller.entities.Remove(this);
                break;
            case MainController.DataType.Structure:
                MapController.controller.entities.Remove(this);
                break;
            case MainController.DataType.Machine:
                MachineController.controller.entities.Remove(this);
                break;
            case MainController.DataType.Mob:
                MobController.controller.entities.Remove(this);
                break;
        }
        if(owner_client != null)
        {
            owner_client.ClearFocusedEntity();
            ClearClientOwner();
        }
        UnloadNetworkEntity();
        if(this is not AbstractTurf) Move();
    }
    public void UnloadNetworkEntity()
    {
        LoadedNetworkEntity?.DeleteEntity();
        internal_loaded_network_entity = null;
    }

    /*****************************************************************
     * Client input and control
     ****************************************************************/
    public virtual AbstractEntity ActiveHand // Used to get activehand from mob, but in any other entity...
    {
        get {return null;}
        set {}
    }
    public void SetClientOwner(NetworkClient client)
    {
        Debug.Assert(client != null);
        owner_client = client;
    }
    public NetworkClient GetClientOwner()
    {
        return owner_client;
    }
    public void ClearClientOwner()
    {
        owner_client = null;
    }
    public virtual void ControlUpdate(Godot.Collections.Dictionary client_input_data)
    {
        if(client_input_data.Keys.Count == 0) return;
        // Got an actual control update!
        MapController.GridPos new_pos = grid_pos;
        Vector2 mover = new Vector2((float)client_input_data["x"].AsDouble(),(float)client_input_data["y"].AsDouble()).Normalized() * MainController.controller.config.input_factor;
        new_pos.hor += mover.X;
        new_pos.ver += mover.Y;
        Move(new_pos);
    }
    // Clicking other entities
    public virtual void Clicked( AbstractEntity used_entity, AbstractEntity target, Godot.Collections.Dictionary click_params) 
    {
        GD.Print(display_name.The(true) + " CLICKED " + target?.display_name.The() + " USING " + used_entity?.display_name.The()); // REPLACE ME!!!
    }
    // Being dragged by other entities to somewhere else
    public virtual void Dragged( AbstractEntity user, AbstractEntity target,Godot.Collections.Dictionary click_params) 
    { 
        GD.Print(user?.display_name.The(true) + " DRAGGED " + display_name.The() + " TO " + target?.display_name.The()); // REPLACE ME!!!
    }
    public virtual string Examine( AbstractEntity user, string infix = "", string suffix = "")
    {
        return "[b]That's " + display_name.AutoPlural() + infix + suffix + ".[/b] " + description;
    }
    public virtual void PointAt(AbstractEntity target, Vector3 pos)
    {
        if(target == null) return;
        AbstractEntity.CreateEntity(MainController.DataType.Effect,"BASE:POINT_AT",new MapController.GridPos(target.GridPos.GetMapID(),pos));
    }

    /*****************************************************************
     * Interaction handling
     ****************************************************************/
    public virtual bool InteractCanReach(AbstractMob user, AbstractEntity target, int range)
    {
        if(MapController.Adjacent(user,target,false)) return true; // Already adjacent.
        return false;
    }

    public bool _Interact( AbstractEntity user, AbstractEntity target, float attack_modifier, Godot.Collections.Dictionary click_parameters)
    {
        // Attempt surgery!
        if(this is AbstractItem used_item && target is AbstractMob target_complexmob)
        {
            /*if(can_operate(this_complexmob, user) && used_item.do_surgery(this_complexmob,user,user.SelectingZone))
                return TRUE*/  // TODO - Surgery hook! =================================================================================================================================
        }
        // Turf interactions!
        if(this is AbstractTurf this_turf) 
        {
            if(user == null) return false;
            if(user is AbstractMob user_mob)
            {
                if(user_mob.SelectingIntent == DAT.Intent.Help)
                {
                    // Be on help intent if you want to click a turf with an item. Construction etc.
                    if(this_turf.InteractTurf( this, user_mob)) return true;
                }
                else
                {
                    // In most cases you'll swing a weapon over a turf, otherwise handle harm intent actions on a turf...
                    if(this_turf.AttackTurf( this, user_mob)) return true;
                }
            }
        }
        // SpecialInteraction returns true if it performs a unique action that does not call RespondToInteraction() on the entity being targeted!
        if(InteractionSpecial( user, target,click_parameters)) return true; 
        // If no overridden actions, perform an  attack!
        return target._RespondToInteraction( user, this, attack_modifier, click_parameters);
    }

    protected bool _RespondToInteraction( AbstractEntity user, AbstractEntity used_entity, float attack_modifier, Godot.Collections.Dictionary click_parameters)
    {
        if(user is not AbstractMob) return false;
        return used_entity.UsedAsWeapon( user, this as AbstractSimpleMob, user.SelectingZone, attack_modifier);
    }


    /*****************************************************************
     * Interaction Overrides, USE THESE, and not the others PLEASE.
     ****************************************************************/
    public virtual void InteractionSelf( AbstractEntity user ) 
    { 
        // What happens when an object is used on itself.
    }

    public virtual bool InteractionSpecial( AbstractEntity user, AbstractEntity target, Godot.Collections.Dictionary click_parameters) 
    {
        return false; //return TRUE to skip calling InteractBy() on target after this proc does stuff, and go straight to AfterAttack()
    }

    public virtual void InteractionUnresolved( AbstractEntity user, AbstractEntity target, bool proximity, Godot.Collections.Dictionary click_parameters)
    {
        // What happens after an attack that misses, or for which InteractionSpecial returned true
    }

    public virtual void InteractionTouched( AbstractEntity user)
    {
        GD.Print(display_name.The(true) + " was touched by " + user.display_name.The());
    }

    // TK interaction...
    public virtual void InteractWithTK( AbstractEntity user)
    {
        // Telekinetic attack: By default, emulate the user's unarmed attack
        if(user is AbstractSimpleMob user_mob)
        {
            if(IsIntangible()) return;
            if(user_mob.Stat != DAT.LifeState.Alive) return;
            user_mob._UnarmedInteract(user,false); // attack_hand, attack_paw, etc
        }
    }
    public virtual void InteractSelfTK( AbstractEntity user)
    {
        // Called when you click the TK grab in your hand, or closets overriding InteractWithTK()
    }


    /*****************************************************************
     * Animation delay handler
     ****************************************************************/
    private bool animation_locked;
    private double animation_lock_time; // time when animation will be unlocked
    public void SetAnimationLock(bool set_lock, double set_animation_delay)
    {
        animation_locked = set_lock;
        float ticks = (float)set_animation_delay * MainController.tick_rate;
        animation_lock_time = MainController.WorldTicks + ticks;
    }
    public bool GetAnimationLock()
    {
        if(animation_locked) return MainController.WorldTicks < animation_lock_time;
        return false;
    }


    /*****************************************************************
     * Attack handling, we've got past just interacting, we're now doing harm!
     ****************************************************************/
    protected virtual bool UsedAsWeapon( AbstractEntity user, AbstractSimpleMob target, DAT.ZoneSelection target_zone, float attack_modifier)
    {
        if(target == user && user.SelectingIntent != DAT.Intent.Hurt) return false;
        if(this is AbstractItem self_item && (self_item.attack_force == 0 || self_item.flags.NOBLUDGEON)) return false;
        if(user is AbstractSimpleMob user_mob)
        {
            /////////////////////////
            user_mob.lastattacked = target;
            target.lastattacker = user;
            ChatController.LogAttack(user?.display_name.The(true) + "attacked" + target?.display_name.The() + " with " + this.display_name.The() + " (INTENT: " + user?.SelectingIntent + ") (DAMTYE: [" + this.damtype + "])");
            /////////////////////////
            user_mob.SetClickCooldown( user_mob.GetAttackCooldown(this) );
            user_mob?.LoadedNetworkEntity?.AnimationRequest(NetwornAnimations.Animation.ID.Attack, MapController.GetMapDirection(user,target) );
        }
        var hit_zone = target.ResolveWeaponHit(this, user, target_zone);
        if(hit_zone != DAT.ZoneSelection.Miss) WeaponHitMobEffect( user, target, hit_zone, attack_modifier);
        return true;
    }
    protected float WeaponHitMobEffect( AbstractEntity user, AbstractSimpleMob target, DAT.ZoneSelection target_zone, float attack_modifier)
    {
        //user.break_cloak() // TODO Cloaking devices ================================================================================================================================
        AudioController.PlayAt(hit_sound, target.grid_pos, AudioController.screen_range, 5);

        float power = attack_force;
        /*
        if(HULK in user.mutations)
        {
            power *= 2;
        }
        */
        power *= attack_modifier; // Insert motionvalues reference here
        return target.HitByWeapon(this, user, power, target_zone);
    }
    protected virtual DAT.ZoneSelection ResolveWeaponHit(AbstractEntity user, AbstractEntity used_item, DAT.ZoneSelection target_zone)
    {
        return target_zone; // Entities that can miss when used to attack. return DAT.ZoneSelection.None, otherwise resolve to another target_zone or return the same target zone if attack was successful.
    }
    public virtual bool AttackedGeneric(AbstractEntity user, int damage, string attack_message)
    {
        return true;
    }

    /*****************************************************************
     * Movement and storage
     ****************************************************************/
    public AbstractTurf GetTurf()
    {
        return MapController.GetTurfAtPosition(grid_pos,true);
    }
    public AbstractEntity GetLocation()
    {
        if(location == null) return GetTurf();
        return location;
    }

    AbstractEntity location = null; 
    private List<AbstractEntity> contains = new List<AbstractEntity>();
    public List<AbstractEntity> Contents
    {
        get {return contains;}
    }
    
    private void EnterLocation(AbstractEntity absLoc)
    {
        Debug.Assert(absLoc != null);
        location = absLoc;
    }
    private void ClearLocation()
    {
        location = null;
    }
    
    private void LeaveOldLoc(bool perform_turf_actions)
    {
        if(location != null)
        {
            // Leave old turf
            AbstractTurf old_turf = location as AbstractTurf;
            old_turf.EntityExited(this,perform_turf_actions);
        }
    }

    public virtual AbstractEntity Move(MapController.GridPos new_grid, bool perform_turf_actions = true)
    {
        // Is new location valid?
        Vector3 dir_vec = MapController.GetMapDirection(grid_pos.WorldPos(),new_grid.WorldPos());
        
        if(MapController.OnSameMap(grid_pos.GetMapID(),new_grid.GetMapID()))
        {
            // EDGE LOCK
            float threshold = (float)0.01;
            if(!MapController.IsTurfValid(new MapController.GridPos(new_grid.GetMapID(),new_grid.hor,grid_pos.ver,grid_pos.dep)))
            {
                if(dir_vec.X < 0)
                {   
                    new_grid.hor = Mathf.Floor(grid_pos.hor) + threshold;
                }
                else if(dir_vec.X > 0)
                {
                    new_grid.hor = Mathf.Floor(grid_pos.hor) + 1 - threshold;
                }
            }
            if(!MapController.IsTurfValid(new MapController.GridPos(new_grid.GetMapID(),grid_pos.hor,new_grid.ver,grid_pos.dep)))
            {
                if(dir_vec.Z < 0)
                {
                    new_grid.ver = Mathf.Floor(grid_pos.ver) + threshold;
                }
                else if(dir_vec.Z > 0)
                {
                    new_grid.ver = Mathf.Floor(grid_pos.ver) + 1 - threshold;
                }
            }
            
            if(!IsIntangible() && !unstoppable) // ghosts, and unstoppable movers do not bump
            {
                // Check to see on each axis if we bump... This allows sliding!
                bool bump_h = false;
                AbstractTurf hor_turf = MapController.GetTurfAtPosition(new MapController.GridPos(new_grid.GetMapID(),new_grid.hor,grid_pos.ver,grid_pos.dep),true);
                if(hor_turf != null && hor_turf != GetTurf() && hor_turf.density)
                {
                    bump_h = true;
                    if(dir_vec.X < 0)
                    {   
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + threshold;
                    }
                    else if(dir_vec.X > 0)
                    {
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + 1 - threshold;
                    }
                }
                bool bump_v = false;
                AbstractTurf ver_turf = MapController.GetTurfAtPosition(new MapController.GridPos(grid_pos.GetMapID(),grid_pos.hor,new_grid.ver,grid_pos.dep),true);
                if(ver_turf != null && ver_turf != GetTurf() && ver_turf.density)
                {
                    bump_v = true;
                    if(dir_vec.Z < 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + threshold;
                    }
                    else if(dir_vec.Z > 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + 1 - threshold;
                    }
                }
                // Bump solids!
                AbstractTurf corner_turf = MapController.GetTurfAtPosition(new_grid,true);
                if(corner_turf != null && corner_turf.density)
                {
                    // Corner bonking is silly... Needs a unique case when you run into a corner exactly head on!
                    MapController.GridPos original_new = new_grid;
                    if(dir_vec.X < 0)
                    {   
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + threshold;
                    }
                    else if(dir_vec.X > 0)
                    {
                        new_grid.hor = Mathf.Floor(grid_pos.hor) + 1 - threshold;
                    }
                    if(dir_vec.Z < 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + threshold;
                    }
                    else if(dir_vec.Z > 0)
                    {
                        new_grid.ver = Mathf.Floor(grid_pos.ver) + 1 - threshold;
                    }
                    Bump(corner_turf);
                    corner_turf.Bump(this);
                    
                    // Randomly break out of direct headon perfect corner intersections...
                    if(TOOLS.Prob(50)) 
                    {
                        new_grid.hor = original_new.hor;
                    }
                    else 
                    {
                        new_grid.ver = original_new.ver;
                    }
                }
                else
                {
                    if(bump_h) // bonk horizontal.
                    {
                        Bump(hor_turf);
                        hor_turf.Bump(this);
                    }
                    if(bump_v) // bonk verticle.
                    {
                        Bump(ver_turf);
                        ver_turf.Bump(this);
                    }
                }
            }  
        } 
        
        // At same location still, don't bother with much else...
        if(location is AbstractTurf && grid_pos.Equals(new_grid)) 
        {
            // Move around in current turf
            grid_pos = new_grid;
            SyncPositionRotation(false,false);
            return location;
        }

        // Leave old location, perform uncrossing events! Enter new turf...
        LeaveOldLoc(perform_turf_actions);
        grid_pos = new_grid;
        // Enter new location!
        AbstractTurf new_turf = MapController.GetTurfAtPosition(grid_pos,true);
        new_turf?.EntityEntered(this,perform_turf_actions);
        UpdateNetwork(false,false);
        return location;
    }
    public AbstractEntity Move(AbstractEntity new_destination, bool perform_turf_actions = true)
    {
        // If in same container, don't bother with entrance/exit actions.
        if(location == new_destination) return location;
        if(new_destination is AbstractTurf)
        {
            // It's a turf! move normally!
            return Move(new_destination.GridPos, perform_turf_actions);
        }
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(perform_turf_actions);
        // Enter new location
        grid_pos = new MapController.GridPos("BAG",Vector3.Zero);
        new_destination.EntityEntered(this,perform_turf_actions);
        UpdateNetwork(false,false);
        return location;
    }
    public AbstractEntity Move() // Move to nullspace
    {
        // Leave old location, perform uncrossing events!
        LeaveOldLoc(false);
        // Enter new location
        grid_pos = new MapController.GridPos("NULL",Vector3.Zero);
        UpdateNetwork(false,false);
        return location;
    }
    public void Drop(AbstractEntity new_destination, AbstractEntity user)
    {
        Move(new_destination,true);
    }
    public void PickedUp(AbstractEntity new_destination, AbstractEntity user)
    {
        Move(new_destination,true);
    }

    // Another entity has entered us...
    public void EntityEntered(AbstractEntity abs, bool perform_action)
    {
        Debug.Assert(abs != null);
        if(perform_action)
        {
            for(int i = 0; i < contains.Count; i++) 
            {
                contains[i].Crossed(abs);
            }
        }
        // Network entity 
        contains.Add(abs);
        abs.EnterLocation(this);
    }
    // An entity stored inside us has gone somewhere else!
    public void EntityExited(AbstractEntity abs, bool perform_action)
    {
        Debug.Assert(abs != null);
        contains.Remove(abs);
        if(perform_action)
        {
            for(int i = 0; i < contains.Count; i++) 
            {
                contains[i].UnCrossed(abs);
            }
        }
        abs.ClearLocation();
    }

    //Returns the storage depth of an atom. This is the number of storage items the atom is contained in before reaching toplevel (the turf).
    //Returns -1 if the atom was not found in a container.
    public int StorageDepth(AbstractEntity container)
    {
        int depth = 0;
        AbstractEntity cur_entity = this;
        while(cur_entity != null && !container.contains.Contains(cur_entity))
        {
            if(cur_entity.location is AbstractTurf) return -1;
            cur_entity = cur_entity.location;
            depth++;
        }
        if(cur_entity == null) return -1;	//inside something with a null location.
        return depth;
    }

    /*****************************************************************
     * Tag control
     ****************************************************************/
    public void SetTag(string new_tag)
    {
        MapController.Internal_UpdateTag(this,new_tag);
        tag = new_tag;
    }
    public void ClearTag() { SetTag(""); }
    public string GetTag() { return tag; }

    /*****************************************************************
     * Network entity spawning/despawning and visibility
     ****************************************************************/
    public bool IsNetworkVisible()
    {
        if(location is AbstractTurf)
        {

            return true; // By default, visible on turfs.
        }
        if(location is AbstractMob)
        {
            // Handle.... HANDS... And other slots.
            // TODO - mob hands and other slots! =================================================================================================================================
            return false;
        }
        return false;
    }
    public void UpdateNetworkDirection(DAT.Dir old_dir)
    {
        if(old_dir != direction) UpdateNetwork(false,false);
    }
    public void UpdateNetwork(bool mesh_update, bool force) // Spawns and despawns currently loaded entity. While calling SyncPositionRotation(bool mesh_update) is cheaper... Calling this is safer.
    {
        if(MapController.IsChunkLoaded(grid_pos.GetMapID(),grid_pos.ChunkPos())) 
        {
            if(this is AbstractTurf) 
            {
                // If turf, update mesh...
                MapController.GetChunk(grid_pos.GetMapID(),grid_pos.ChunkPos()).MeshUpdate();
                return;
            }
            if(location == null) return; // nullspace vanish
            // Otherwise, what does our behavior say?
            bool is_vis = IsNetworkVisible();
            if(is_vis && LoadedNetworkEntity == null)
            {
                internal_loaded_network_entity = NetworkEntity.CreateEntity( this, entity_type, grid_pos.GetMapID());
                SyncPositionRotation(true,true);
                UpdateCustomNetworkData();
                return;
            }
            if(!is_vis && LoadedNetworkEntity != null)
            {
                UnloadNetworkEntity();
                return;
            }
            if(is_vis && LoadedNetworkEntity != null)
            {
                SyncPositionRotation(mesh_update,force);
                return;
            }
        }
        else
        {
            // Entity loaded in an unloaded chunk?
            if(LoadedNetworkEntity != null)
            {
                UnloadNetworkEntity();
                return;
            }
        }
    }
    private void SyncPositionRotation(bool mesh_update, bool force) // Updates position and rotation of currently loaded entity
    {
        if(LoadedNetworkEntity == null) return;
        LoadedNetworkEntity.SetUpdatedPosition(grid_pos.WorldPos(),force);
        LoadedNetworkEntity.direction = direction;
        if(mesh_update) LoadedNetworkEntity.MeshUpdate();
    }

    /*****************************************************************
     * Conditions
     ****************************************************************/
    public virtual bool IsIntangible()
    {
        return intangible;
    }

    public virtual bool IsAnchored()
    {
        return false;
    }

    public virtual bool IsRobotModule()
    {
        return false;
    }
}
