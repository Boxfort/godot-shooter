using Godot;
using System;

public class PlayerHUD : CanvasLayer
{
    Label health;
    Label armor;
    Label weapon;
    Label ammo;
    Label interactText;
    Label message;

    WeaponManager weaponManager;
    PlayerManager playerManager;
    InteractRayCast interactRayCast;

    bool showingMessage = false;
    float messageTimer = 0;
    const float messageTime = 2;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        weapon = GetNode<Label>("DebugPanel/Weapon");
        ammo = GetNode<Label>("UI/Ammo/AmmoCount");
        health = GetNode<Label>("UI/Health/HealthCount");
        armor = GetNode<Label>("UI/Armor/ArmorCount");
        interactText = GetNode<Label>("UI/InteractText");
        message = GetNode<Label>("UI/Message");

        weaponManager = GetNode<WeaponManager>("../Head/Hand");
        playerManager = GetNode<PlayerManager>("../PlayerManager");
        interactRayCast = weaponManager.GetNode<InteractRayCast>("InteractRayCast");

        weaponManager.Connect("OnAmmoChanged", this, nameof(OnAmmoChanged));
        weaponManager.Connect("OnWeaponEquipped", this, nameof(OnWeaponEquipped));
        playerManager.Connect("OnHealthChanged", this, nameof(OnHealthChanged));
        playerManager.Connect("OnArmorChanged", this, nameof(OnArmorChanged));
        playerManager.Connect("OnSecretFound", this, nameof(OnSecretFound));

        interactRayCast.Connect("OnRaycastEnter", this, nameof(OnInteractTextShow));
        interactRayCast.Connect("OnRaycastExit", this, nameof(OnInteractTextHide));

        OnHealthChanged(playerManager.Health);
        OnArmorChanged(playerManager.Armor);
    }

    public override void _Process(float delta)
    {
        if (showingMessage)
        {
            if (messageTimer < messageTime)
            {
                messageTimer += delta;
            }
            else
            {
                showingMessage = false;
                HideMessage();
            }
        }
    }

    private void ShowMessage(string message)
    {
        this.message.Text = message;
        messageTimer = 0.0f;
        showingMessage = true;
        this.message.Show();
    }

    private void HideMessage()
    {
        this.message.Text = "";
        messageTimer = 0.0f;
        showingMessage = false;
        this.message.Hide();
    }

    private void OnSecretFound()
    {
        ShowMessage("SECRET FOUND");
    }

    private void OnInteractTextShow(string text)
    {
        interactText.Text = text;
        interactText.Show();
    }

    private void OnInteractTextHide()
    {
        interactText.Hide();
        interactText.Text = "";
    }

    private void OnAmmoChanged(int ammoCount)
    {
        ammo.Text = ammoCount.ToString();
    }

    private void OnWeaponEquipped(WeaponType weaponType, int ammoCount)
    {
        ammo.Text = ammoCount.ToString();
        weapon.Text = weaponType.ToString();
    }

    private void OnHealthChanged(int newHealth)
    {
        health.Text = newHealth.ToString();
    }

    private void OnArmorChanged(int newArmor)
    {
        armor.Text = newArmor.ToString();
    }
}
