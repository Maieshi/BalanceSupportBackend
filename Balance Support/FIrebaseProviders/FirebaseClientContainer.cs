using FireSharp;
using FireSharp.Interfaces;

namespace Balance_Support;

public class FirebaseClientContainer
{
    private IFirebaseClient client;

    public IFirebaseClient Client => client;
    
    public FirebaseClientContainer()
    {
        this.client = client;
    }
}