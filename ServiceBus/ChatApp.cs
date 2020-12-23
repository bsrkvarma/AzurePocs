class Program
    {
        static void Main(string[] args)
        {
            var serviceBusConnectionString = "";
            var topicPath = "ChatTopic";

            var manager = new ManagementClient(serviceBusConnectionString);
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

            var subClient = new SubscriptionClient(serviceBusConnectionString, topicPath, description);
            subClient.RegisterMessageHandler(ProcessMessagesAsync, Exceptionhandler);
            while (true)
            {
                var chat = Console.ReadLine();
                if (chat == "exit")
                    break;
                var data = JsonConvert.SerializeObject(new { message = chat });
                var chatMessage = new Message(Encoding.UTF8.GetBytes(data)) { Label = description };
                topicClient.SendAsync(chatMessage).Wait();
            }
            //Close the clients
            topicClient.CloseAsync().Wait();
            subClient.CloseAsync().Wait();
            manager.CloseAsync().Wait();
        }

        private async static Task ProcessMessagesAsync(Message msg, CancellationToken arg2)
        {
            var text = Encoding.UTF8.GetString(msg.Body);
            Console.WriteLine($"{msg.Label}>{text}");
        }


        private static Task Exceptionhandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }
    }
