using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yitter.IdGenerator;

namespace Nop.Infrastructure
{
    /// <summary>
    /// 雪花算法进阶版【Yitter.IdGenerator】
    /// </summary>
    public class SnowflakeId
    {
        private static readonly byte workerIdBitLength = 6;

        static SnowflakeId()
        {
            long maxWorkerId = -1L ^ (-1L << workerIdBitLength);

            var random = new Random();

            var workerId = random.Next((int)maxWorkerId);

            var options = new IdGeneratorOptions((ushort)workerId)
            {
                SeqBitLength = 6,// 6生成为15位, 10为16位
                WorkerIdBitLength = workerIdBitLength
            };

            YitIdHelper.SetIdGenerator(options);
        }

        /// <summary>
        /// 生成Id 15 位
        /// </summary>
        /// <returns></returns>
        public static long NextId()
        {
            return YitIdHelper.NextId();
        }
    }
}
