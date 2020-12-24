class Program
    {
        static void Main(string[] args)
        {
            //Service bus connection string
            var serviceBusConnectionString = "";
            //provide valid topic name
            var topicPath = "ChatTopic";
            
            //Management Clinet to interact with service bus
            var manager = new ManagementClient(serviceBusConnectionString);
            //Create a topic to hold the subscription
            if (!manager.TopicExistsAsync(topicPath).Result)
            {
                manager.CreateTopicAsync(topicPath).Wait();
            }

            Console.WriteLine("Welcome to the chatting app, please give your name");
            var description = Console.ReadLine();
            var topicDescription = new SubscriptionDescription(topicPath, description)
            {
                AutoDeleteOnIdle = TimeSpan.FromDays(7)
            };
            
            //Create a subscription for the topic
            if (!manager.SubscriptionExistsAsync(topicPath, description).Result)
            {
                var subscriptionCreateRes = manager.CreateSubscriptionAsync(topicDescription);
                subscriptionCreateRes.Wait();
            }

            var message = new Message
            {
                ContentType = "application/json;charset=utf-8",
                Body = Encoding.UTF8.GetBytes($"{description} entered chat room!"),
                Label = description
            };
            var topicClient = new TopicClient(serviceBusConnectionString, topicPath);
            var messageResult = topicClient.SendAsync(message);
            messageResult.Wait();

            //Subscription handler to receive the 
            var subClient = new SubscriptionClient(serviceBusConnectionString, topicPath, description);
            subClient.RegisterMessageHandler(ProcessMessagesAsync, Exceptionhandler);
            
            //Listen to the console
            while (true)
            {
                var chat = Console.ReadLine();
                if (chat == "exit")
                    break;
                //Post the chat message to the subscription
                var data = JsonConvert.SerializeObject(new { message = chat });
                var chatMessage = new Message(Encoding.UTF8.GetBytes(data)) { Label = description };
                topicClient.SendAsync(chatMessage).Wait();
            }
            
            //Dispose the clients
            topicClient.CloseAsync().Wait();
            subClient.CloseAsync().Wait();
            manager.CloseAsync().Wait();
        }
          
        //Process the message
        private async static Task ProcessMessagesAsync(Message msg, CancellationToken arg2)
        {
            var text = Encoding.UTF8.GetString(msg.Body);
            Console.WriteLine($"{msg.Label}>{text}");
        }


        private static Task Exceptionhandler(ExceptionReceivedEventArgs arg)
        {
                Debug.WriteLine(arg.Exception);
                throw new NotImplementedException();
        }
    }
