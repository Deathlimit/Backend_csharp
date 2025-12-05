using Dapper;
using Lab1Try2.DAL.Interfaces;
using Lab1Try2.DAL.Models;

namespace Lab1Try2.DAL.Repositories
{
    public class AuditLogOrderRepository : IAuditLogOrderRepository
    {
        private readonly UnitOfWork _unitOfWork;

        public AuditLogOrderRepository(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<V1AuditLogOrderDal[]> BulkInsert(V1AuditLogOrderDal[] auditLogs, CancellationToken token)
        {
            var connection = await _unitOfWork.GetConnection(token);

            const string sql = @"
        INSERT INTO audit_log_order 
        (order_id, order_item_id, customer_id, order_status, created_at, updated_at)
        VALUES 
        (@OrderId, @OrderItemId, @CustomerId, @OrderStatus, @CreatedAt, @UpdatedAt)
        RETURNING *";

            // Вариант 1: Выполняем вставку для каждого элемента отдельно ПОМЕНЯТЬ НА БАЛК
            var results = new List<V1AuditLogOrderDal>();

            foreach (var log in auditLogs)
            {
                var result = await connection.QuerySingleAsync<V1AuditLogOrderDal>(
                    new CommandDefinition(sql, log, cancellationToken: token));
                results.Add(result);
            }

            return results.ToArray();
        }

        public async Task<V1AuditLogOrderDal[]> Query(QueryAuditLogOrderDalModel model, CancellationToken token)
        {
            // Убеждаемся, что соединение инициализировано
            var connection = await _unitOfWork.GetConnection(token);

            var sql = @"
                SELECT * FROM audit_log_order 
                WHERE 1=1";

            var parameters = new DynamicParameters();

            if (model.Ids?.Length > 0)
            {
                sql += " AND id = ANY(@Ids)";
                parameters.Add("Ids", model.Ids);
            }

            if (model.OrderIds?.Length > 0)
            {
                sql += " AND order_id = ANY(@OrderIds)";
                parameters.Add("OrderIds", model.OrderIds);
            }

            if (model.CustomerIds?.Length > 0)
            {
                sql += " AND customer_id = ANY(@CustomerIds)";
                parameters.Add("CustomerIds", model.CustomerIds);
            }

            if (model.OrderStatuses?.Length > 0)
            {
                sql += " AND order_status = ANY(@OrderStatuses)";
                parameters.Add("OrderStatuses", model.OrderStatuses);
            }

            sql += " ORDER BY created_at DESC LIMIT @Limit OFFSET @Offset";
            parameters.Add("Limit", model.Limit);
            parameters.Add("Offset", model.Offset);

            var result = await connection.QueryAsync<V1AuditLogOrderDal>(
                new CommandDefinition(sql, parameters, cancellationToken: token));

            return result.ToArray();
        }
    }
}