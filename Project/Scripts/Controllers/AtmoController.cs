public partial class AtmoController : DeligateController
{
    public static AtmoController controller;

    public override bool CanInit()
    {
        return IsSubControllerInit(MapController.controller); // waiting on the map controller first
    }

    public override bool Init()
    {
        controller = this;
        FinishInit();
        return true;
    }

    public override void Fire()
    {
        
    }

    public override void Shutdown()
    {
        
    }
}

