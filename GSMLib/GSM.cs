﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Management;

using GsmComm.GsmCommunication;
using GsmComm.Interfaces;
using GsmComm.PduConverter;
using GsmComm.Server;


namespace GSMLib
{
    public class GSM
    {

        /* Call this method to send a message */
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
        public List<List<string>> ReceiveMessages(string identifier)
        {
            string storage = GetMessageStorage();

            List<List<string>> allMessages = new List<List<string>>();
            List<string> messageInfo = new List<string>();
            
            switch (idenfier)
            {
                case "All":
                    try
                    {
                        DecodedShortMessage[] messages = CommSetting.comm.ReadMessages(PhoneMessageStatus.All, storage);

                        foreach(DecodedShortMessage message in messages)
                        {
                            messageInfo.Add(message.Status);
                            messageInfo.Add(String.Format("Location: {0}/{1}", message.Storage, message.Index));
                            messageInfo.Add(MessageDetails(message.Data));

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
                            messageInfo.Add(message.Status);
                            messageInfo.Add(String.Format("Location: {0}/{1}", message.Storage, message.Index));
                            messageInfo.Add(MessageDetails(message.Data));

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
                            messageInfo.Add(message.Status);
                            messageInfo.Add(String.Format("Location: {0}/{1}", message.Storage, message.Index));
                            messageInfo.Add(MessageDetails(message.Data));

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
                            messageInfo.Add(message.Status);
                            messageInfo.Add(String.Format("Location: {0}/{1}", message.Storage, message.Index));
                            messageInfo.Add(MessageDetails(message.Data));

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
                            messageInfo.Add(message.Status);
                            messageInfo.Add(String.Format("Location: {0}/{1}", message.Storage, message.Index));
                            messageInfo.Add(MessageDetails(message.Data));

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
                    string err = String.Format("Unknown Indentifier '{0}'", indentifier);

                    messageInfo.Add(err);
                    allMessages.Add(err);
                    break;

            }           
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
            Dictionary<string, string> messageDetails = Dictionary<string, string>();

			if (pdu is SmsSubmitPdu)
			{
				// Stored (sent/unsent) message
				SmsSubmitPdu data = (SmsSubmitPdu)pdu;

				string recipient = Convert.ToString(data.DestinationAddress);
				string message = Convert.ToString(data.UserDataText);

                messageDetails.Add("recipient", recipient);
                messageDetails.Add("message", message);
				
				return messageDetails;
			}
			else if (pdu is SmsDeliverPdu)
			{
				// Received message
				SmsDeliverPdu data = (SmsDeliverPdu)pdu;

				string sender = data.OriginatingAddress.ToString();
                string timeStamp = data.OriginatingSCTimestamp.ToString();
                string message = data.UserDataText.ToString();

                messageDetails.Add("sender", sender);
                messageDetails.Add("timeStamp" timeStamp);
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
                if(comm.IsOpen() == true)
                {
                    comm.Close();
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