using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace BasicBot.Services
{
    public class MessageHandlerService
    {
        private static Dictionary<IUserMessage, TaskMessage> TaskEdits = new Dictionary<IUserMessage, TaskMessage>();
        private static Dictionary<IMessageChannel, List<TaskMessage>> TaskMessages = new Dictionary<IMessageChannel, List<TaskMessage>>();
        private static Dictionary<ITextChannel, List<IMessage>> TaskDeletes = new Dictionary<ITextChannel, List<IMessage>>();
        private static Timer timer;
        public void StartTimers()
        {
            Console.WriteLine(DateTime.Now);
            timer = new Timer(_ => OnCallBack(), null, 1000 * 1, Timeout.Infinite); //start in 1 second
        }

        private void OnCallBack()
        {
            MessageEdits();
            SendMessages();
            DeleteMessages();

            timer = new Timer(_ => OnCallBack(), null, 1000 * 1, Timeout.Infinite); //repeat in 1 second//*/
        }

        private void Messages()
        {
            while (TaskEdits.Count != 0)
            {
                var a = TaskEdits.First();

                TaskEdits.Remove(a.Key);
            }
        }

        private void MessageEdits()
        {
            while (TaskEdits.Count != 0)
            {
                var a = TaskEdits.First();

                if (a.Value.Embed != null)//this method does two calls to edit the message, if anyone can figure out how to make it one ill be thankfull
                    a.Key.ModifyAsync(x => x.Embed = a.Value.Embed);
                if (a.Value.Msg != null)
                    a.Key.ModifyAsync(x => x.Content = a.Value.Msg);

                TaskEdits.Remove(a.Key);
            }
        }

        private void SendMessages()
        {
            while (TaskMessages.Where(x => x.Value.Count != 0).Count() != 0)
            {
                var a = TaskMessages.Where(x => x.Value.Count != 0).First();

                while (a.Value.Count != 0)
                {
                    var b = a.Value.First();
                    a.Key.SendMessageAsync(b.Msg, false, b.Embed);

                    a.Value.Remove(b);
                }
            }
        }

        private void DeleteMessages()
        {
            while (TaskDeletes.Count != 0)
            {
                var a = TaskDeletes.First();
                a.Key.DeleteMessagesAsync(a.Value);

                TaskDeletes.Remove(a.Key);
            }
        }

        public class TaskMessage
        {
            public string Msg { get; set; } = null;
            public Embed Embed { get; set; } = null;
        }

        public class TaskDelete
        {
            public List<IMessage> Msgs { get; set; } = new List<IMessage>();
            public SocketTextChannel Chnl { get; set; } = null;
        }

        public static void AddEditToMessage(IUserMessage userMsg, string newMsg = null, Embed embed = null)
        {
            TaskEdits[userMsg] = new TaskMessage { Msg = newMsg, Embed = embed };
        }

        #region AddMessageToBeDeleted overides
        public static void AddMessageToBeDeleted(IUserMessage msg, ITextChannel chnl) =>
            AddMessageToBeDeleted(msg as IMessage, chnl);
        public static void AddMessageToBeDeleted(IUserMessage msg, SocketTextChannel chnl) =>
            AddMessageToBeDeleted(msg as IMessage, chnl as ITextChannel);
        public static void AddMessageToBeDeleted(SocketUserMessage msg, ITextChannel chnl) =>
            AddMessageToBeDeleted(msg as IMessage, chnl);
        public static void AddMessageToBeDeleted(SocketUserMessage msg, SocketTextChannel chnl) =>
            AddMessageToBeDeleted(msg as IMessage, chnl as ITextChannel);

        public static void AddMessageToBeDeleted(IEnumerable<IUserMessage> msgs, ITextChannel chnl) =>
            AddMessageToBeDeleted(msgs as List<IMessage>, chnl);
        public static void AddMessageToBeDeleted(IEnumerable<IUserMessage> msgs, SocketTextChannel chnl) =>
            AddMessageToBeDeleted(msgs as List<IMessage>, chnl as ITextChannel);
        public static void AddMessageToBeDeleted(IEnumerable<SocketUserMessage> msgs, ITextChannel chnl) =>
            AddMessageToBeDeleted(msgs as List<IMessage>, chnl);
        public static void AddMessageToBeDeleted(IEnumerable<SocketUserMessage> msgs, SocketTextChannel chnl) =>
            AddMessageToBeDeleted(msgs as List<IMessage>, chnl as ITextChannel);
        #endregion AddMessageToBeDeleted overides

        public static void AddMessageToBeDeleted(IMessage msg, ITextChannel chnl) =>
            AddMessageToBeDeleted(new List<IMessage> { msg }, chnl);

        public static void AddMessageToBeDeleted(IEnumerable<IMessage> msgs, ITextChannel chnl)
        {
            if (!TaskDeletes.ContainsKey(chnl)) //check if chanl dictionary exist yet
                TaskDeletes[chnl] = new List<IMessage>();

            var taskDelChnl = TaskDeletes[chnl];
            taskDelChnl.AddRange(msgs);
        }

        public static void AddMessageToBeSent(IMessageChannel chnl, string Msg = null, Embed embed = null) =>
            AddMessageToBeSent(chnl ,new List<TaskMessage> { new TaskMessage { Embed = embed, Msg = Msg } });


        public static void AddMessageToBeSent(IMessageChannel chnl, IEnumerable<TaskMessage> msgs)
        {
            if (!TaskMessages.ContainsKey(chnl)) //check if chanl dictionary exist yet
                TaskMessages[chnl] = new List<TaskMessage>();

            var taskMesChnl = TaskMessages[chnl];
            taskMesChnl.AddRange(msgs);
        }
    }
}

