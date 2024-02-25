public partial class MapController : DeligateController
{
    public static MapController controller;

    public override bool CanInit()
    {
        return true;
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
