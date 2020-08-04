using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using GsmComm.GsmCommunication;
using GsmComm.Interfaces;
using GsmComm.PduConverter;
using GsmComm.Server;


namespace GSMLib
{
    public class GSM
    {

        /* Call this method to send a message */
        /// <summary>
        /// Method for sending message basing on phone number and sms message
        /// <summary/>
        /// <param name="cellNumber">
        /// A string of the phone number to send the message to
        /// <param/>
        /// <param name="smsMessage">
        /// A string of the text Message to send to the phone number
        /// <param/>
        public void SendMessage(string cellNumber, string smsMessage)
        {
            
            SmsSubmitPdu pdu;

            string CellNumber = cellNumber.ToString();
            string SMSMessage = smsMessage.ToString();

            try
            {
                if(CommSetting.comm.IsConnected() == true)
                {
                    try
                    {
                        pdu = new SmsSubmitPdu(SMSMessage, CellNumber, "");
                        CommSetting.comm.SendMessage(pdu);
                    }
                    catch (Exception e)
                    {
                        /* Message showing error to send the message */
                        //Console.WriteLine("Failed to send Message: " + e.Message);
                        CommSetting.comm.Close();
                    }
                }
                else
                {
                    /* Message to show that no GSM device is connected */
                    //Console.WriteLine("No GSM device connected");
                }
            }
            catch (Exception e)
            {
                /*Message to show that there is a connection error */
                //Console.WriteLine("There was a connected error, try and check your connection. " + e.Message);
            }
            
        }

        /* Call this method to recieve messages */
        /// <summary>
        /// Method returns a list of dictionaries containing information regarding the sms received
        /// </summary>
        /// <param name="identifier">
        /// Identifier can be a string of values 
        /// All - Returns a List of dictionaries of information pertaining all the SMS received
        /// Read - Returns a List of dictionaries of information pertaining the read SMSs
        /// Unread - Returns a List of dictionaries of information pertaining the unread SMSs
        /// Sent - Returns a List of dictionaries of information pertaining the send SMSs
        /// Unsent - Returns a List of dictionaries of information pertaining the unsent SMSs
        /// </param>
        /// <returns>
        /// A list containing a dictionary of information regarding each text Message
        /// keys vary with the type of identifier used
        /// <seealso cref="MessageDetails" />
        /// <returns/>
        public List<Dictionary<string, string>> ReceiveMessages(string identifier)
        {
            string storage = GetMessageStorage();

            List<Dictionary<string, string>> allMessages = new List<Dictionary<string, string>>();
            Dictionary<string,string> messageInfo = new Dictionary<string, string>();
            
            switch (identifier)
            {
                case "All":
                    try
                    {
                        DecodedShortMessage[] messages = CommSetting.comm.ReadMessages(PhoneMessageStatus.All, storage);

                        foreach(DecodedShortMessage message in messages)
                        {
                            messageInfo = MessageDetails(message.Data);
                            messageInfo.Add("storage", Convert.ToString(message.Storage));
                            messageInfo.Add("index", Convert.ToString(message.Index));
                            messageInfo.Add("status", Convert.ToString(message.Status));

                            allMessages.Add(messageInfo);				
                        }

                    }
                    catch(Exception e)
                    {
                        /* To show failure to read all messages from the storage location */
                        //Console.WriteLine(e.Message);
                    }
                    break;
                
                case "Read":
                    try
                    {
                        DecodedShortMessage[] messages = CommSetting.comm.ReadMessages(PhoneMessageStatus.ReceivedRead, storage);

                        foreach(DecodedShortMessage message in messages)
                        {
                            messageInfo = MessageDetails(message.Data);
                            messageInfo.Add("storage", Convert.ToString(message.Storage));
                            messageInfo.Add("index", Convert.ToString(message.Index));
                            messageInfo.Add("status", Convert.ToString(message.Status));

                            allMessages.Add(messageInfo);				
                        }

                    }
                    catch(Exception e)
                    {
                        /* To show failure to read received messages from the storage location */
                        //Console.WriteLine(e.Message);
                    }
                    break;
                
                case "Unread":
                    try
                    {
                        DecodedShortMessage[] messages = CommSetting.comm.ReadMessages(PhoneMessageStatus.ReceivedUnread, storage);

                        foreach(DecodedShortMessage message in messages)
                        {
                            messageInfo = MessageDetails(message.Data);
                            messageInfo.Add("storage", Convert.ToString(message.Storage));
                            messageInfo.Add("index", Convert.ToString(message.Index));
                            messageInfo.Add("status", Convert.ToString(message.Status));

                            allMessages.Add(messageInfo);				
                        }

                    }
                    catch(Exception e)
                    {
                        /* To show failure to read unread messages from the storage location */
                        //Console.WriteLine(e.Message);
                    }
                    break;

                case "Sent":
                    try
                    {
                        DecodedShortMessage[] messages = CommSetting.comm.ReadMessages(PhoneMessageStatus.StoredSent, storage);
                        
                        foreach(DecodedShortMessage message in messages)
                        {
                            messageInfo = MessageDetails(message.Data);
                            messageInfo.Add("storage", Convert.ToString(message.Storage));
                            messageInfo.Add("index", Convert.ToString(message.Index));
                            messageInfo.Add("status", Convert.ToString(message.Status));

                            allMessages.Add(messageInfo);			
                        }

                    }
                    catch(Exception e)
                    {
                        /* To show failure to read sent messages from the storage location */
                        //Console.WriteLine(e.Message);
                    }
                    break;
                
                case "Unsent":
                    try
                    {
                        DecodedShortMessage[] messages = CommSetting.comm.ReadMessages(PhoneMessageStatus.StoredUnsent, storage);

                        foreach(DecodedShortMessage message in messages)
                        {
                            messageInfo = MessageDetails(message.Data);
                            messageInfo.Add("storage", Convert.ToString(message.Storage));
                            messageInfo.Add("index", Convert.ToString(message.Index));
                            messageInfo.Add("status", Convert.ToString(message.Status));
                            allMessages.Add(messageInfo);				
                        }

                    }
                    catch(Exception e)
                    {
                        /* To show failure to read unsent messages from the storage location */
                        //Console.WriteLine(e.Message);
                    }
                    break;
                default:
                    string err = String.Format("Unknown Identifier '{0}'", identifier);

                    messageInfo.Add("Error", "Unknown Identifier");
                    
                    allMessages.Add(messageInfo);
                    break;

            }

            return allMessages;           
        }
              

        private string GetMessageStorage()
		{
			string storage = string.Empty;

			storage = PhoneStorageType.Sim;
				
            //storage = PhoneStorageType.Phone;
            //Need a way to alternate between the two storage types.
            
			if (storage.Length == 0) throw new ApplicationException("Unknown message storage.");
			else return storage;
		}

        private Dictionary<string, string> MessageDetails(SmsPdu pdu)
		{
            Dictionary<string, string> messageDetails = new Dictionary<string, string>();

			if (pdu is SmsSubmitPdu)
			{
				// Stored (sent/unsent) message
				SmsSubmitPdu data = (SmsSubmitPdu)pdu;

				string recipient = data.DestinationAddress.ToString();
				string message = data.UserDataText.ToString();

                messageDetails.Add("recipient", recipient);
                messageDetails.Add("message", message);
				
				return messageDetails;
			}
			else if (pdu is SmsDeliverPdu)
			{
				// Received message
				SmsDeliverPdu data = (SmsDeliverPdu)pdu;

				string sender = data.OriginatingAddress.ToString();
                string timeStamp = data.SCTimestamp.ToString();
                string message = data.UserDataText.ToString();

                messageDetails.Add("sender", sender);
                messageDetails.Add("timeStamp", timeStamp);
                messageDetails.Add("message", message);

				return messageDetails;
			}
			else if (pdu is SmsStatusReportPdu)
			{
				// Status report
				SmsStatusReportPdu data = (SmsStatusReportPdu)pdu;

                string recipient = data.RecipientAddress;
                string status = data.Status.ToString();
                string timeStamp = data.DischargeTime.ToString();
                string messageRef = data.MessageReference.ToString();

                messageDetails.Add("recipient", recipient);
                messageDetails.Add("status", status);
                messageDetails.Add("timeStamp", timeStamp);
                messageDetails.Add("messageRef", messageRef);
				
				return messageDetails;
			}

            else return messageDetails;
            /* Error Message showing unknown message type */
			//Console.WriteLine("Unknown message type: " + pdu.GetType().ToString());
		}

         /* Logic to call when connection needs to be closed */
        public void CloseGSMConnection()
        {
            try
            {
                if(CommSetting.comm.IsOpen() == true)
                {
                    CommSetting.comm.Close();
                }
            }
            catch (Exception e)
            {
                /* Message to show error closing connection */
                //Console.WriteLine("Error closing connection: " + e.Message);
            }
        }
    }
}
