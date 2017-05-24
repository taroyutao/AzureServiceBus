using Microsoft.Azure;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;

namespace TopicSendAndReceiveWithFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            string topicName = "yutopic";//主题名称
            //服务总线连接字符串，在Portal上直接获取
            string servicebusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            NamespaceManager nm = NamespaceManager.CreateFromConnectionString(servicebusConnectionString);

            //删除已经存在的同名主题
            if (nm.TopicExists(topicName))
            {
                nm.DeleteTopic(topicName);
            }

            TopicDescription topic = new TopicDescription(topicName);
            string keyB = SharedAccessAuthorizationRule.GenerateRandomKey();
            string keyc = SharedAccessAuthorizationRule.GenerateRandomKey();

            topic.Authorization.Add(new SharedAccessAuthorizationRule("FromPartB", keyB, new AccessRights[] { AccessRights.Listen }));
            topic.Authorization.Add(new SharedAccessAuthorizationRule("FromPartC", keyc, new AccessRights[] { AccessRights.Listen }));

            //创建的主题，默认并不包含分区
            nm.CreateTopic(topic);

            Console.WriteLine("B部门访问秘钥：" + keyB);
            Console.WriteLine("C部门访问秘钥：" + keyc);

            nm.CreateSubscription(topicName, "high_value", new SqlFilter("value >= 1000"));
            nm.CreateSubscription(topicName, "low_value", new SqlFilter("value < 1000"));

            TopicClient client = TopicClient.CreateFromConnectionString(servicebusConnectionString, topicName);

            int orderNumber = 1;
            while (true)
            {
                Console.Write("输入第" + orderNumber + "号订单金额：");
                string value = Console.ReadLine();
                int orderValue = 0;
                if (int.TryParse(value, out orderValue))
                {
                    var message = new BrokeredMessage();
                    message.Properties.Add("order_number", orderNumber);
                    message.Properties.Add("value", orderValue);
                    client.Send(message);
                    Console.WriteLine("第" + orderNumber + "号订单已经发出!");
                    orderNumber++;
                }
                else
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("无效的输入，请重新输入。");
                    }
                }
            }
        }
    }
}

