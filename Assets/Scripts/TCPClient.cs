using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using UnityEngine;

public class TCPClient : MonoBehaviour
{
    private TcpClient socketConnection; 	
	private NetworkStream stream;
	private Thread clientReceiveThread;
	private Boolean meessageSent;
	private int frame;
	InputHandler localPlayerInput;
	InputHandler netPlayerInput;
    // Start is called before the first frame update
    void Start()
    {
        setupTCPServer();
		frame = 0;

		GameObject np = GameObject.Find("Net Player");
		netPlayerInput = np.GetComponent<InputHandler>();
		
    }

	void setupTCPServer() {
		try {
			socketConnection = new TcpClient("72.211.223.101", 3653); 
			Byte[] bytes = new Byte[1024];
			stream = socketConnection.GetStream();
		} catch (Exception e) {
			Debug.Log("Socket error" + e);
		}
	}

	string ReceiveFrameData() {
		int numberOfBytesRead = 0;
		byte[] data = new byte[1024];
		if (stream.CanRead) {
			try
            {
                //data available always false?
                //Debug.Log("data availability:  " + theStream.DataAvailable);
                numberOfBytesRead = stream.Read(data, 0, data.Length);  
                string receiveMsg = System.Text.Encoding.ASCII.GetString(data, 0, numberOfBytesRead);
				InputObject inputObj = JsonUtility.FromJson<InputObject>(receiveMsg);
				netPlayerInput.setInputObject(inputObj);
                //Debug.Log("receive msg:  " + receiveMsg + " Frame: " + inputObj.jumpPressed);
				
				return receiveMsg;
            }
            catch(Exception e)
            {
                Debug.Log("Error in NetworkStream: " + e);
            }
		} 

		return "";
	}
    private void SendMessage(string outgoingMessage) { 
		Debug.Log(socketConnection);        
		if (socketConnection == null) {             
			return;         
		}  		
		
		try { 			
			// Get a stream object for writing. 			
			if (stream.CanWrite) {                 

	
				/*Test testObject = new Test();
				testObject.Hello = new Hello("Joe");
				string output = JsonUtility.ToJson(testObject) + "\n";
				Debug.Log(output);*/

				// Convert string message to byte array.                 
				byte[] clientMessageAsByteArray = Encoding.ASCII.GetBytes(outgoingMessage); 				
				// Write byte array to socketConnection stream.                 
				stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);                 
				Debug.Log("Client sent his message - should be received by server");             
			}         
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		}
	} 

    // Update is called once per frame
    public void netUpdate(InputObject inputMessage)
    {
		string outgoingMessage = JsonUtility.ToJson(inputMessage) + "\n";
		SendMessage(outgoingMessage);
		ReceiveFrameData();
        frame++;
    }
}
