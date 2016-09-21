using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.VR.WSA.Input;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

[Serializable]
public class ObjectState
{
    public float devPosx;
    public float devPosy;
    public float devPosz;
    public float devQx;
    public float devQy;
    public float devQz;
    public float devQw;
    public float spPosx;
    public float spPosy;
    public float spPosz;
    public float spQx;
    public float spQy;
    public float spQz;
    public float spQw;
    public float shPosx;
    public float shPosy;
    public float shPosz;
    public float shQx;
    public float shQy;
    public float shQz;
    public float shQw;
}


[Serializable]
public class RemoteState
{
    public float devPosx;
    public float devPosy;
    public float devPosz;
    public float devQx;
    public float devQy;
    public float devQz;
    public float devQw;
    public bool devGhostOn;
    public float shPosx;
    public float shPosy;
    public float shPosz;
    public float shQx;
    public float shQy;
    public float shQz;
    public float shQw;
    public bool shGhostOn;
    public float spPosx;
    public float spPosy;
    public float spPosz;
    public float spQx;
    public float spQy;
    public float spQz;
    public float spQw;
    public bool spGhostOn;
    public float handPosx;
    public float handPosy;
    public float handPosz;
    public float handQx;
    public float handQy;
    public float handQz;
    public float handQw;
    public bool handOn;
}

class SocketHandler
{
    public static ObjectState currentState = new ObjectState();
    public static RemoteState remoteState = new RemoteState();
    public static Mutex stateLock = new Mutex();
    public static Mutex remoteStateLock = new Mutex();
    public static Mutex messageLock = new Mutex();

    public class Message
    {
        public byte type;
        public byte[] data;
    }

    public static List<Message> messageList = new List<Message>();

    void processMessages(SocketAsyncEventArgs e)
    {
        if (e.Buffer.Length <= 0)
        {
            return;
        }

        int index = 0;
        int length = BitConverter.ToInt32(e.Buffer, index);
        index += 4;
        while(length > 0)
        {
            //Debug.Log("Message length: " + length);
            byte type = e.Buffer[index];
            index++;

            switch(type)
            {
                case 21:
                    //Debug.Log("Got send state request");

                    Message m1 = new Message();
                    m1.type = 20;

                    stateLock.WaitOne();
                    string jsonStr = JsonUtility.ToJson(currentState);
                    stateLock.ReleaseMutex();

                    m1.data = Encoding.ASCII.GetBytes(jsonStr);

                    messageLock.WaitOne();

                    messageList.Add(m1);

                    messageLock.ReleaseMutex();

                    break;
                case 22:
                    //Debug.Log("Got remote state");
                    byte[] data = new byte[length-1];
                    Array.Copy(e.Buffer, index, data, 0, length - 1);
                    string dataStr = System.Text.Encoding.UTF8.GetString(data);

                    remoteStateLock.WaitOne();

                    JsonUtility.FromJsonOverwrite(dataStr, remoteState
                        );

                    remoteStateLock.ReleaseMutex();

                    index = index - 1 + length;

                    // request next update
                    Message m = new Message();
                    m.type = 19;
                    m.data = null;

                    messageLock.WaitOne();

                    messageList.Add(m);

                    messageLock.ReleaseMutex();

                    break;
                default:
                    Debug.Log("unknown message type: " + type);
                    break;
            }

            length = BitConverter.ToInt32(e.Buffer, index);
            index += 4;
        }
    }

    void sendMessages(Socket socket)
    {
        int totallen = 0;

        messageLock.WaitOne();

        // no messages to send, wait for next remote message
        if(messageList.Count <= 0)
        {
            byte[] buf = new byte[4];
            SocketAsyncEventArgs e2 = new SocketAsyncEventArgs();
            e2.SetBuffer(buf, 0, 4);
            e2.Completed += RecvSizeCallback;
            if (!socket.ReceiveAsync(e2))
            {
                RecvSizeCallback(socket, e2);
            }
            messageLock.ReleaseMutex();
            return;
        }

        foreach(Message m in messageList)
        {
            // len + type
            totallen += 5;

            if (m.data != null)
            {
                totallen += m.data.Length;
            }
        }

        // terminator
        totallen += 4;
        int payloadsize = totallen + 4;

        int index = 0;

        byte[] payload = new byte[payloadsize];
        byte[] intbuffer = BitConverter.GetBytes(totallen);
        intbuffer.CopyTo(payload, index);
        index += 4;

        foreach(Message m in messageList)
        {
            int mylen = 1;
            if(m.data != null)
            {
                mylen += m.data.Length;
            }

            intbuffer = BitConverter.GetBytes(mylen);
            intbuffer.CopyTo(payload, index);
            index += 4;

            payload[index] = m.type;
            index++;

            if(m.data != null)
            {
                m.data.CopyTo(payload, index);
                index += m.data.Length;
            }
        }
        intbuffer = BitConverter.GetBytes(0);
        intbuffer.CopyTo(payload, index);

        messageList.Clear();
        messageLock.ReleaseMutex();


        SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
        e1.SetBuffer(payload, 0, payload.Length);
        e1.Completed += SendCallback;
        if (!socket.SendAsync(e1))
        {
            SendCallback(socket, e1);
        }
    }

    void connectionError()
    {
        Debug.Log("Got socket error");
    }

    private void RecvDataCallback(object recver, SocketAsyncEventArgs e)
    {
        //Debug.Log("Got data");

        Socket recvSocket = (Socket)recver;
        if (e.SocketError != SocketError.Success)
        {
            e.Dispose();
            connectionError();
            return;
        }

        processMessages(e);

        e.Dispose();

        sendMessages(recvSocket);
    }

    private void RecvSizeCallback(object recver, SocketAsyncEventArgs e)
    {
        //Debug.Log("Got size");

        Socket recvSocket = (Socket)recver;
        if (e.SocketError != SocketError.Success)
        {
            e.Dispose();
            connectionError();
            return;
        }

        int size = BitConverter.ToInt32(e.Buffer, 0);
        //Debug.Log("Size: " + size);

        e.Dispose();

        byte[] recvData = new byte[size];
        SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
        e1.SetBuffer(recvData, 0, size);
        e1.Completed += RecvDataCallback;
        if(!recvSocket.ReceiveAsync(e1))
        {
            RecvDataCallback(recvSocket, e1);
        }
    }

    private void SendCallback(object sender, SocketAsyncEventArgs e)
    {
        //Debug.Log("Send done");

        Socket senderSocket = (Socket)sender;
        if (e.SocketError != SocketError.Success)
        {
            e.Dispose();
            connectionError();
            return;
        }

        e.Dispose();
        
        byte[] buf = new byte[4];
        SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
        e1.SetBuffer(buf, 0, 4);
        e1.Completed += RecvSizeCallback;
        if(!senderSocket.ReceiveAsync(e1))
        {
            RecvSizeCallback(senderSocket, e1);
        }

    }

    private void AcceptCallback(object sender, SocketAsyncEventArgs e)
    {
        Debug.Log("accept callback");
        Socket listenSocket = (Socket)sender;
        do
        {
            try
            {
                Debug.Log("Got connection");
                Socket aSocket = e.AcceptSocket;
                aSocket.NoDelay = true;

                stateLock.WaitOne();
                string jsonStr = JsonUtility.ToJson(currentState);
                stateLock.ReleaseMutex();

                byte[] data = Encoding.ASCII.GetBytes(jsonStr);
                Debug.Log("Data: " + jsonStr + " size: " + data.Length);

                byte[] length = BitConverter.GetBytes(data.Length+1);
                byte[] reqlen = BitConverter.GetBytes(1);
                byte[] termlen = BitConverter.GetBytes(0);
                byte[] totallen = BitConverter.GetBytes(reqlen.Length + termlen.Length + 1);
                byte[] payload = new byte[data.Length + length.Length + 1 + reqlen.Length + termlen.Length + totallen.Length + 1];

                length.CopyTo(payload, 0);
                data.CopyTo(payload, length.Length);
                payload[data.Length + length.Length] = (byte)'\0';
                totallen.CopyTo(payload, data.Length + length.Length + 1);
                reqlen.CopyTo(payload, data.Length + length.Length + 1 + totallen.Length);
                payload[data.Length + length.Length + 1 + totallen.Length + reqlen.Length] = 19;
                termlen.CopyTo(payload, data.Length + length.Length + 1 + totallen.Length + reqlen.Length + 1);

                //Debug.Log("length: " + BitConverter.ToString(length));
                //Debug.Log("data: " + BitConverter.ToString(data));
                Debug.Log("payload: " + BitConverter.ToString(payload));

                SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
                e1.SetBuffer(payload, 0, payload.Length);
                e1.Completed += SendCallback;
                if(!aSocket.SendAsync(e1))
                {
                    SendCallback(aSocket, e1);
                }
            }
            catch
            {
                // handle any exceptions here;
                Debug.Log("Got exception");
            }
            finally
            {
                e.AcceptSocket = null; // to enable reuse
                Debug.Log("Finally");
            }

            Debug.Log("Listen for next connection");
        } while (!listenSocket.AcceptAsync(e));
    }

    public void run()
    {
        Debug.Log("Creating socket");
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.NoDelay = true;
        //server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 8765);

        try
        {
            server.Bind(localEndPoint);
            server.Listen(10);

            Debug.Log("Waiting for a connection...");
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += AcceptCallback;
            if (!server.AcceptAsync(e))
            {
                AcceptCallback(server, e);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }
}

[Serializable]
public class TrackingState
{
    public bool m0v;
    public float m0px;
    public float m0py;
    public float m0pz;
    public float m0qx;
    public float m0qy;
    public float m0qz;
    public float m0qw;
    public bool m1v;
    public float m1px;
    public float m1py;
    public float m1pz;
    public float m1qx;
    public float m1qy;
    public float m1qz;
    public float m1qw;
    public bool m2v;
    public float m2px;
    public float m2py;
    public float m2pz;
    public float m2qx;
    public float m2qy;
    public float m2qz;
    public float m2qw;
    public bool m3v;
    public float m3px;
    public float m3py;
    public float m3pz;
    public float m3qx;
    public float m3qy;
    public float m3qz;
    public float m3qw;
}

class TrackingHandler
{
    public static Mutex activeLock = new Mutex();
    public static Mutex stateLock = new Mutex();
    public bool connActive;
    public static TrackingState state = new TrackingState();

    private bool getStateByNumber(int index, ref Vector3 position, ref Quaternion rotation)
    {
        if (index < 0 || index > 3)
        {
            return false;
        }

        switch(index)
        {
            case 0:
                position.x = state.m0px;
                position.y = state.m0py;
                position.z = state.m0pz;
                rotation.x = state.m0qx;
                rotation.y = state.m0qy;
                rotation.z = state.m0qz;
                rotation.w = state.m0qw;
                return state.m0v;
            case 1:
                position.x = state.m1px;
                position.y = state.m1py;
                position.z = state.m1pz;
                rotation.x = state.m1qx;
                rotation.y = state.m1qy;
                rotation.z = state.m1qz;
                rotation.w = state.m1qw;
                return state.m1v;
            case 2:
                position.x = state.m2px;
                position.y = state.m2py;
                position.z = state.m2pz;
                rotation.x = state.m2qx;
                rotation.y = state.m2qy;
                rotation.z = state.m2qz;
                rotation.w = state.m2qw;
                return state.m2v;
            case 3:
                position.x = state.m3px;
                position.y = state.m3py;
                position.z = state.m3pz;
                rotation.x = state.m3qx;
                rotation.y = state.m3qy;
                rotation.z = state.m3qz;
                rotation.w = state.m3qw;
                return state.m3v;
            default:
                return false;

        }
    }

    private void socketError()
    {
        activeLock.WaitOne();
        connActive = false;
        activeLock.ReleaseMutex();
    }

    private void RecvDataCallback(object recver, SocketAsyncEventArgs e)
    {
        //Debug.Log("Got data");

        Socket socket = (Socket)recver;
        if (e.SocketError != SocketError.Success)
        {
            e.Dispose();
            socketError();
            return;
        }

        string dataStr = System.Text.Encoding.UTF8.GetString(e.Buffer);

        //Debug.Log("Data: " + dataStr);

        e.Dispose();

        stateLock.WaitOne();
        JsonUtility.FromJsonOverwrite(dataStr, state);

        //Debug.Log("V: " + state.m1v + " P: " + state.m1px + " " + state.m1py + " " + state.m1pz);

        SocketHandler.stateLock.WaitOne();

        // put valid tracker state data into object state
        int devId = 1;
        Vector3 pos = new Vector3();
        Quaternion rot = new Quaternion();
        bool valid = getStateByNumber(devId, ref pos, ref rot);
        if(valid)
        {
            //Debug.Log("Setting value");
            //Debug.Log("Pos: " + pos.x + " " + pos.y + " " + pos.z);
            SocketHandler.currentState.devPosx = pos.x;
            SocketHandler.currentState.devPosy = pos.y;
            SocketHandler.currentState.devPosz = pos.z;
            SocketHandler.currentState.devQx = rot.x;
            SocketHandler.currentState.devQy = rot.y;
            SocketHandler.currentState.devQz = rot.z;
            SocketHandler.currentState.devQw = rot.w;
        }

        int spongeId = 2;
        valid = getStateByNumber(spongeId, ref pos, ref rot);
        if (valid)
        {
            //Debug.Log("Setting value");
            //Debug.Log("Pos: " + pos.x + " " + pos.y + " " + pos.z);
            SocketHandler.currentState.spPosx = pos.x;
            SocketHandler.currentState.spPosy = pos.y;
            SocketHandler.currentState.spPosz = pos.z;
            SocketHandler.currentState.spQx = rot.x;
            SocketHandler.currentState.spQy = rot.y;
            SocketHandler.currentState.spQz = rot.z;
            SocketHandler.currentState.spQw = rot.w;
        }

        int sheetId = 3;
        valid = getStateByNumber(sheetId, ref pos, ref rot);
        if (valid)
        {
            //Debug.Log("Setting value");
            //Debug.Log("Pos: " + pos.x + " " + pos.y + " " + pos.z);
            SocketHandler.currentState.shPosx = pos.x;
            SocketHandler.currentState.shPosy = pos.y;
            SocketHandler.currentState.shPosz = pos.z;
            SocketHandler.currentState.shQx = rot.x;
            SocketHandler.currentState.shQy = rot.y;
            SocketHandler.currentState.shQz = rot.z;
            SocketHandler.currentState.shQw = rot.w;
        }

        SocketHandler.stateLock.ReleaseMutex();

        stateLock.ReleaseMutex();

        // small wait to not flood the network ~120Hz
        using (EventWaitHandle tmpEvent = new ManualResetEvent(false))
        {
            tmpEvent.WaitOne(TimeSpan.FromMilliseconds(4));
        }

        byte[] message = new byte[1];
        message[0] = (byte)21;

        SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
        e1.SetBuffer(message, 0, message.Length);
        e1.Completed += SendCallback;
        if (!socket.SendAsync(e1))
        {
            SendCallback(socket, e1);
        }
    }

    private void RecvSizeCallback(object recver, SocketAsyncEventArgs e)
    {
        //Debug.Log("Got size");

        Socket socket = (Socket)recver;
        if (e.SocketError != SocketError.Success)
        {
            e.Dispose();
            Debug.Log("Failed to recv");
            socketError();
            return;
        }

        int size = BitConverter.ToInt32(e.Buffer, 0);
        //Debug.Log("Size: " + size);

        e.Dispose();

        byte[] recvData = new byte[size];
        SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
        e1.SetBuffer(recvData, 0, size);
        e1.Completed += RecvDataCallback;
        if (!socket.ReceiveAsync(e1))
        {
            RecvDataCallback(socket, e1);
        }
    }

    private void SendCallback(object sender, SocketAsyncEventArgs e)
    {
        //Debug.Log("Send done");

        Socket socket = (Socket)sender;
        if (e.SocketError != SocketError.Success)
        {
            e.Dispose();
            Debug.Log("Failed to send");
            socketError();
            return;
        }

        e.Dispose();

        byte[] buf = new byte[4];
        SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
        e1.SetBuffer(buf, 0, 4);
        e1.Completed += RecvSizeCallback;
        if (!socket.ReceiveAsync(e1))
        {
            RecvSizeCallback(socket, e1);
        }
    }

    private void ConnectCallback(object sender, SocketAsyncEventArgs e)
    {
        Debug.Log("Connect done");

        Socket socket = (Socket)sender;
        if (e.SocketError != SocketError.Success)
        {
            e.Dispose();
            Debug.Log("Failed to connect");
            socketError();
            return;
        }

        e.Dispose();

        byte[] message = new byte[1];
        message[0] = (byte)21;

        SocketAsyncEventArgs e1 = new SocketAsyncEventArgs();
        e1.SetBuffer(message, 0, message.Length);
        e1.Completed += SendCallback;
        if (!socket.SendAsync(e1))
        {
            SendCallback(socket, e1);
        }
    }

    public void run()
    {
        state.m1v = false;

        connActive = false;
        while (true)
        {
            Debug.Log("Creating Tracking socket");
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server.NoDelay = true;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("137.110.119.175"), 8877);

            try
            {
                Debug.Log("Connecting to server");
                connActive = true;
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.RemoteEndPoint = endPoint;
                e.Completed += ConnectCallback;
                if (!server.ConnectAsync(e))
                {
                    ConnectCallback(server, e);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }

            while(true)
            {
                activeLock.WaitOne();
                if(!connActive)
                {
                    activeLock.ReleaseMutex();
                    break;
                }
                activeLock.ReleaseMutex();
            }

            using (EventWaitHandle tmpEvent = new ManualResetEvent(false))
            {
                tmpEvent.WaitOne(TimeSpan.FromSeconds(5));
            }
        }
    }
}

public class RootScript : MonoBehaviour {
    bool placing = true;
    GestureRecognizer recognizer;
    SocketHandler socketHandler;
    TrackingHandler trackingHandler;
    Task sTask;
    Task tTask;

    void InitState(SocketHandler handler)
    {
        Transform dev = this.gameObject.transform.Find("Device");
        if (dev != null)
        {
            SocketHandler.currentState.devPosx = dev.localPosition.x;
            SocketHandler.currentState.devPosy = dev.localPosition.y;
            SocketHandler.currentState.devPosz = dev.localPosition.z;
            SocketHandler.currentState.devQx = dev.localRotation.x;
            SocketHandler.currentState.devQy = dev.localRotation.y;
            SocketHandler.currentState.devQz = dev.localRotation.z;
            SocketHandler.currentState.devQw = dev.localRotation.w;
        }
        else
        {
            Debug.Log("Cannot find Device");
        }

        dev = this.gameObject.transform.Find("Sponge");
        if (dev != null)
        {
            SocketHandler.currentState.spPosx = dev.localPosition.x;
            SocketHandler.currentState.spPosy = dev.localPosition.y;
            SocketHandler.currentState.spPosz = dev.localPosition.z;
            SocketHandler.currentState.spQx = dev.localRotation.x;
            SocketHandler.currentState.spQy = dev.localRotation.y;
            SocketHandler.currentState.spQz = dev.localRotation.z;
            SocketHandler.currentState.spQw = dev.localRotation.w;
        }
        else
        {
            Debug.Log("Cannot find Sponge");
        }

        dev = this.gameObject.transform.Find("Sheet");
        if (dev != null)
        {
            SocketHandler.currentState.shPosx = dev.localPosition.x;
            SocketHandler.currentState.shPosy = dev.localPosition.y;
            SocketHandler.currentState.shPosz = dev.localPosition.z;
            SocketHandler.currentState.shQx = dev.localRotation.x;
            SocketHandler.currentState.shQy = dev.localRotation.y;
            SocketHandler.currentState.shQz = dev.localRotation.z;
            SocketHandler.currentState.shQw = dev.localRotation.w;
        }
        else
        {
            Debug.Log("Cannot find Sheet");
        }

        SocketHandler.remoteState.devGhostOn = false;
    }

    void InitState(TrackingHandler handler)
    {
    }

    // Use this for initialization
    void Start () {
        recognizer = new GestureRecognizer();
        recognizer.TappedEvent += (source, tapCount, ray) =>
        {
            // Send an OnSelect message to the focused object and its ancestors.
            placing = !placing;
        };
        recognizer.StartCapturingGestures();

        socketHandler = new SocketHandler();

        sTask = new Task(new Action(socketHandler.run));
        InitState(socketHandler);
        sTask.Start();

        trackingHandler = new TrackingHandler();

        tTask = new Task(new Action(trackingHandler.run));
        InitState(trackingHandler);
        tTask.Start();
    }
	
	// Update is called once per frame
	void Update () {
	    // If the user is in placing mode,
        // update the placement to match the user's gaze.

        if (placing)
        {
            // Do a raycast into the world that will only hit the Spatial Mapping mesh.
            var headPosition = Camera.main.transform.position;
            var gazeDirection = Camera.main.transform.forward;

            //Debug.Log("Checking gaze\n");

            RaycastHit hitInfo;
            if (Physics.Raycast(headPosition, gazeDirection, out hitInfo,
                30.0f, Physics.DefaultRaycastLayers))
            {
                //Debug.Log("Hit\n");
                // Move this object's parent object to
                // where the raycast hit the Spatial Mapping mesh.
                this.gameObject.transform.position = hitInfo.point;

                // Rotate this object's parent object to face the user.
                Quaternion toQuat = Camera.main.transform.localRotation;
                toQuat.x = 0;
                toQuat.z = 0;
                this.gameObject.transform.rotation = toQuat;
            }
        }
        
        // update from remote state
        SocketHandler.stateLock.WaitOne();

        Transform dev = this.gameObject.transform.Find("Device");
        if (dev != null)
        {
            Vector3 position = dev.localPosition;
            position.x = SocketHandler.currentState.devPosx;
            position.y = SocketHandler.currentState.devPosy;
            position.z = SocketHandler.currentState.devPosz;
            dev.localPosition = position;

            //Debug.Log("LocalPos: " + dev.localPosition.x + " " + dev.localPosition.y + " " + dev.localPosition.z);

            Quaternion rotation = dev.localRotation;
            rotation.x = SocketHandler.currentState.devQx;
            rotation.y = SocketHandler.currentState.devQy;
            rotation.z = SocketHandler.currentState.devQz;
            rotation.w = SocketHandler.currentState.devQw;
            dev.localRotation = rotation;
        }
        else
        {
            Debug.Log("Cannot find Device");
        }

        dev = this.gameObject.transform.Find("Sponge");
        if (dev != null)
        {
            Vector3 position = dev.localPosition;
            position.x = SocketHandler.currentState.spPosx;
            position.y = SocketHandler.currentState.spPosy;
            position.z = SocketHandler.currentState.spPosz;
            dev.localPosition = position;

            //Debug.Log("LocalPos: " + dev.localPosition.x + " " + dev.localPosition.y + " " + dev.localPosition.z);

            Quaternion rotation = dev.localRotation;
            rotation.x = SocketHandler.currentState.spQx;
            rotation.y = SocketHandler.currentState.spQy;
            rotation.z = SocketHandler.currentState.spQz;
            rotation.w = SocketHandler.currentState.spQw;
            dev.localRotation = rotation;
        }
        else
        {
            Debug.Log("Cannot find Sponge");
        }

        dev = this.gameObject.transform.Find("Sheet");
        if (dev != null)
        {
            Vector3 position = dev.localPosition;
            position.x = SocketHandler.currentState.shPosx;
            position.y = SocketHandler.currentState.shPosy;
            position.z = SocketHandler.currentState.shPosz;
            dev.localPosition = position;

            //Debug.Log("LocalPos: " + dev.localPosition.x + " " + dev.localPosition.y + " " + dev.localPosition.z);

            Quaternion rotation = dev.localRotation;
            rotation.x = SocketHandler.currentState.shQx;
            rotation.y = SocketHandler.currentState.shQy;
            rotation.z = SocketHandler.currentState.shQz;
            rotation.w = SocketHandler.currentState.shQw;
            dev.localRotation = rotation;
        }
        else
        {
            Debug.Log("Cannot find Sheet");
        }

        SocketHandler.stateLock.ReleaseMutex();

        SocketHandler.remoteStateLock.WaitOne();

        dev = this.gameObject.transform.Find("DeviceGhost");
        Transform linet = this.gameObject.transform.Find("DeviceLine");
        if (dev != null && SocketHandler.remoteState.devGhostOn)
        {
            dev.gameObject.SetActive(true);
            Vector3 position = dev.localPosition;
            position.x = SocketHandler.remoteState.devPosx;
            position.y = SocketHandler.remoteState.devPosy;
            position.z = SocketHandler.remoteState.devPosz;
            dev.localPosition = position;

            //Debug.Log("LocalPos: " + dev.localPosition.x + " " + dev.localPosition.y + " " + dev.localPosition.z);

            Quaternion rotation = dev.localRotation;
            rotation.x = SocketHandler.remoteState.devQx;
            rotation.y = SocketHandler.remoteState.devQy;
            rotation.z = SocketHandler.remoteState.devQz;
            rotation.w = SocketHandler.remoteState.devQw;
            dev.localRotation = rotation;

            if(linet != null)
            {
                LineRenderer lr = linet.gameObject.GetComponent<LineRenderer>();
                if(lr != null)
                {
                    Transform dev2 = this.gameObject.transform.Find("Device");
                    if (dev2 != null)
                    {
                        Vector3 pos1 = new Vector3();
                        pos1.x = dev.localPosition.x;
                        pos1.y = dev.localPosition.y;
                        pos1.z = dev.localPosition.z;

                        Vector3 pos2 = new Vector3();
                        pos2.x = dev2.localPosition.x;
                        pos2.y = dev2.localPosition.y;
                        pos2.z = dev2.localPosition.z;

                        lr.SetPosition(0, pos1);
                        lr.SetPosition(1, pos2);

                        linet.gameObject.SetActive(true);
                    }
                }
            }

        }
        else if(dev != null)
        {
            dev.gameObject.SetActive(false);
            if(linet != null)
            {
                linet.gameObject.SetActive(false);
            }
        }

        dev = this.gameObject.transform.Find("SheetGhost");
        linet = this.gameObject.transform.Find("SheetLine");
        if (dev != null && SocketHandler.remoteState.shGhostOn)
        {
            dev.gameObject.SetActive(true);
            Vector3 position = dev.localPosition;
            position.x = SocketHandler.remoteState.shPosx;
            position.y = SocketHandler.remoteState.shPosy;
            position.z = SocketHandler.remoteState.shPosz;
            dev.localPosition = position;

            //Debug.Log("LocalPos: " + dev.localPosition.x + " " + dev.localPosition.y + " " + dev.localPosition.z);

            Quaternion rotation = dev.localRotation;
            rotation.x = SocketHandler.remoteState.shQx;
            rotation.y = SocketHandler.remoteState.shQy;
            rotation.z = SocketHandler.remoteState.shQz;
            rotation.w = SocketHandler.remoteState.shQw;
            dev.localRotation = rotation;

            if (linet != null)
            {
                LineRenderer lr = linet.gameObject.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    Transform dev2 = this.gameObject.transform.Find("Sheet");
                    if (dev2 != null)
                    {
                        Vector3 pos1 = new Vector3();
                        pos1.x = dev.localPosition.x;
                        pos1.y = dev.localPosition.y;
                        pos1.z = dev.localPosition.z;

                        Vector3 pos2 = new Vector3();
                        pos2.x = dev2.localPosition.x;
                        pos2.y = dev2.localPosition.y;
                        pos2.z = dev2.localPosition.z;

                        lr.SetPosition(0, pos1);
                        lr.SetPosition(1, pos2);

                        linet.gameObject.SetActive(true);
                    }
                }
            }

        }
        else if (dev != null)
        {
            dev.gameObject.SetActive(false);
            if (linet != null)
            {
                linet.gameObject.SetActive(false);
            }
        }

        dev = this.gameObject.transform.Find("SpongeGhost");
        linet = this.gameObject.transform.Find("SpongeLine");
        if (dev != null && SocketHandler.remoteState.spGhostOn)
        {
            dev.gameObject.SetActive(true);
            Vector3 position = dev.localPosition;
            position.x = SocketHandler.remoteState.spPosx;
            position.y = SocketHandler.remoteState.spPosy;
            position.z = SocketHandler.remoteState.spPosz;
            dev.localPosition = position;

            //Debug.Log("LocalPos: " + dev.localPosition.x + " " + dev.localPosition.y + " " + dev.localPosition.z);

            Quaternion rotation = dev.localRotation;
            rotation.x = SocketHandler.remoteState.spQx;
            rotation.y = SocketHandler.remoteState.spQy;
            rotation.z = SocketHandler.remoteState.spQz;
            rotation.w = SocketHandler.remoteState.spQw;
            dev.localRotation = rotation;

            if (linet != null)
            {
                LineRenderer lr = linet.gameObject.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    Transform dev2 = this.gameObject.transform.Find("Sponge");
                    if (dev2 != null)
                    {
                        Vector3 pos1 = new Vector3();
                        pos1.x = dev.localPosition.x;
                        pos1.y = dev.localPosition.y;
                        pos1.z = dev.localPosition.z;

                        Vector3 pos2 = new Vector3();
                        pos2.x = dev2.localPosition.x;
                        pos2.y = dev2.localPosition.y;
                        pos2.z = dev2.localPosition.z;

                        lr.SetPosition(0, pos1);
                        lr.SetPosition(1, pos2);

                        linet.gameObject.SetActive(true);
                    }
                }
            }

        }
        else if (dev != null)
        {
            dev.gameObject.SetActive(false);
            if (linet != null)
            {
                linet.gameObject.SetActive(false);
            }
        }

        dev = this.gameObject.transform.Find("handXForm");
        if (dev != null && SocketHandler.remoteState.handOn)
        {
            dev.gameObject.SetActive(true);
            Vector3 position = dev.localPosition;
            position.x = SocketHandler.remoteState.handPosx;
            position.y = SocketHandler.remoteState.handPosy;
            position.z = SocketHandler.remoteState.handPosz;
            dev.localPosition = position;

            //Debug.Log("LocalPos: " + dev.localPosition.x + " " + dev.localPosition.y + " " + dev.localPosition.z);

            Quaternion rotation = dev.localRotation;
            rotation.x = SocketHandler.remoteState.handQx;
            rotation.y = SocketHandler.remoteState.handQy;
            rotation.z = SocketHandler.remoteState.handQz;
            rotation.w = SocketHandler.remoteState.handQw;
            dev.localRotation = rotation;
        }
        else if (dev != null)
        {
            dev.gameObject.SetActive(false);
        }

        SocketHandler.remoteStateLock.ReleaseMutex();
    }
}
