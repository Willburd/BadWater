using Godot;
using System;
using System.Collections.Generic;

public partial class AccountController : DeligateController
{
    public class Account
    {
        public string id_name;
        public NetworkClient active_client;
        public AbstractEntity registered_entity;
        public string password_hash;
    }


    private static List<Account> loaded_accounts = new List<Account>();


    public static bool CanJoin(string assign_name, string pass_hash)
    {
        // Check if already in use
        foreach(Account acc in loaded_accounts)
        {
            if(acc.id_name == assign_name)
            {
                // client inactive, we are rejoining! Check password of course
                if(acc.active_client == null)
                {
                    if(acc.password_hash == pass_hash) 
                    {
                        GD.Print("-" + assign_name + " was allowed to join");
                        return true;
                    }
                    GD.Print("-Could not join as " + assign_name + " password mismatch");
                    return false;
                } 
                GD.Print("-Could not join as " + assign_name + " already active client");
                return false;
            }
        }
        // New account trying to join!
        GD.Print("-FRESH Account: " + assign_name);
        if(!MainController.controller.config.allow_new_accounts) return false; // Disallow new accounts!
        return true;
    }


    public static bool JoinGame(NetworkClient client, string assign_name, string pass_hash)
    {
        // check if an old account
        for(int i = 0; i < loaded_accounts.Count; i++) 
        {
            Account acc_check = loaded_accounts[i];
            if(acc_check.id_name == assign_name && acc_check.active_client == null)
            {
                if(acc_check.password_hash == pass_hash)
                {
                    GD.Print("-Account: " + acc_check.id_name + " correct password, client set to " + client.Name);
                    acc_check.active_client = client;
                    return true;
                }
                GD.Print("-Account: " + acc_check.id_name + " failed to validate, password mismatch");
                return false; // Somehow not the same password as before.
            }
        }
        // Fresh account joining
        GD.Print("-Fresh account " + assign_name + " initializing...");
        Account acc = new Account
        {
            id_name = assign_name,
            active_client = client,
            password_hash = pass_hash
        };
        loaded_accounts.Add(acc);
        return true;
    }


    public static void UpdateAccount(NetworkClient client, AbstractEntity focusedEnt)
    {
        // Update our account with client's current status, so we can rejoin properly if we get DCed
        for(int i = 0; i < loaded_accounts.Count; i++) 
        {
            Account acc = loaded_accounts[i];
            if(acc.active_client == client)
            {
                GD.Print("Account " + acc.id_name + " reserved entity updated to " + focusedEnt);
                acc.registered_entity = focusedEnt;
                return;
            }
        }
        if(!client.has_logged_in)
        {
            // Not yet logged, try and log them!
            if(client.login_name == null) 
            {
                // Request client info
                client.RequestCredentials();
            }
        }
        else
        {
            // How did you get on the server without an account?
            GD.Print("Client had no account to update... Disconnecting " + client.Name);
            client.DisconnectClient();
        }
    }


    public static void ClientLeave(NetworkClient client)
    {
        GD.Print("Client DC");
        for(int i = 0; i < loaded_accounts.Count; i++) 
        {
            Account acc = loaded_accounts[i];
            if(acc.active_client == client)
            {
                GD.Print("-Account: " + acc.id_name + " client cleared");
                acc.active_client = null;
                return;
            }
        }
    }


    public static AbstractEntity GetClientEntity(NetworkClient client)
    {
        GD.Print("Client requesting saved entitiy");
        for(int i = 0; i < loaded_accounts.Count; i++) 
        {
            Account acc = loaded_accounts[i];
            if(acc.active_client == client)
            {
                GD.Print("-Account: " + acc.id_name + " reserved entity reloaded: " + loaded_accounts[i].registered_entity);
                return loaded_accounts[i].registered_entity;
            }
        }
        return null;
    }


    public override bool CanInit()
    {
        return true; // waiting on the Atmo controller, and by proxy: Map and Chem controllers!
    }

    public override bool Init()
    {
        tick_rate = -1; // NO TICK
        controller = this;
        return true;
    }

    public override void SetupTick()
    {
        FinishInit();
    }

    public override void Fire()
    {
        //GD.Print(Name + " Fired");
    }

    public override void Shutdown()
    {
        
    }
}
