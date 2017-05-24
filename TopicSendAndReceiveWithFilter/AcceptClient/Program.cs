using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;

namespace AcceptClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("请输入您的访问秘钥：");
            string key = Console.ReadLine();

            //通过SAS访问订阅
            Uri uri = new Uri("sb://yutaoservicebustestnotdelete.servicebus.chinacloudapi.cn");
            MessagingFactory factory = MessagingFactory.Create(uri, TokenProvider.CreateSharedAccessSignatureTokenProvider("FromPartB", key));

            SubscriptionClient client = factory.CreateSubscriptionClient("yutopic", "high_value");
            Console.WriteLine("等待消息...");
            while(true)
            {
                var message = client.Receive();
                if (message != null)
                {
                    Console.WriteLine(string.Format("接收到订单：#{0}。价值：{1}",
                        message.Properties["order_number"],
                        message.Properties["value"]));
                    message.Complete();
                }
            }
        }
    }
}
