using FplClient.Data;
using FplManager.Infrastructure.Constants;
using FplManager.Infrastructure.Extensions;
using System;

namespace FplManager.Application.Services
{
    public class TransferApprovalService
    {
        public bool IsTransferApproved()
        {
            Console.WriteLine($"To Approve of Transfer, type 'y':");
            var ch = Console.ReadKey().KeyChar;
            if (ch.Equals('y'))
            {
                Console.WriteLine("\nMaking transfer");
                return true;
            }
            else
            {
                Console.WriteLine("\nCancelling transfer");
                return false;
            }
        }
    }
}
