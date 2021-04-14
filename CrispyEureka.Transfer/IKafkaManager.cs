using System.Threading.Tasks;

namespace CrispyEureka.Transfer
{
    public interface IKafkaManager
    {
        Task InitTopic(string topicName);
    }
}