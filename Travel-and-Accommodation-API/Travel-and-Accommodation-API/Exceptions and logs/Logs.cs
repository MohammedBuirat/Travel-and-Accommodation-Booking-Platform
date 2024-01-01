namespace Travel_and_Accommodation_API.Exceptions_and_logs
{
    public static class Logs<T> where T : class
    {
        public static void GetEntitiesLog(ILogger logger, string type)
        {
            logger.LogInformation($"{type} were retrieved");
        }
        public static void GetEntitiesException(ILogger logger, Exception ex, string type)
        {
            logger.LogError(ex, $"Error while retrieving {type}");
        }

        public static void GetEntityLog(ILogger logger, string type, Guid id)
        {
            logger.LogInformation($"{type} with id {id} was retrieved");
        }
        public static void GetEntityException(ILogger logger, Exception ex, string type, Guid id)
        {
            logger.LogError(ex, $"Error while retrieving {type} with id {id}");
        }

        public static void DeleteEntityLog(ILogger logger, string type, T entity)
        {
            logger.LogInformation($"{type} was deleted " +
                $"{entity}");
        }
        public static void DeleteEntityException(ILogger logger, Exception ex, string type, Guid id)
        {
            logger.LogError(ex, $"Error while deleting {type} with id {id}");
        }

        public static void UpdateEntityLog(ILogger logger, string type, T entity)
        {
            logger.LogInformation($"{type} was updated "+
                $" old {type}: {entity}");
        }
        public static void UpdateEntityException(ILogger logger, Exception ex, string type, Guid id)
        {
            logger.LogError(ex, $"Error while updating {type} with id {id}");
        }

        public static void AddEntityLog(ILogger logger, string type, T entity)
        {
            logger.LogInformation($"New {type} was inserted {entity}");
        }
        public static void AddEntityException(ILogger logger, Exception ex, string type, T entity)
        {
            logger.LogError(ex, $"Error while Inserting new {type}   {entity}");
        }

        public static void GetEntityLog(ILogger logger, string type, Guid id, Guid id2)
        {
            logger.LogInformation($"{type} with ids {id},{id2} was retrieved");
        }
        public static void GetEntityException(ILogger logger, Exception ex, string type, Guid id, Guid id2)
        {
            logger.LogError(ex, $"Error while retrieving {type} with ids {id},{id2}");
        }

        public static void DeleteEntityException(ILogger logger, Exception ex, string type, Guid id, Guid id2)
        {
            logger.LogError(ex, $"Error while deleting {type} with id {id},{id2}");
        }

        public static void UpdateEntityException(ILogger logger, Exception ex, string type, Guid id, Guid id2)
        {
            logger.LogError(ex, $"Error while updating {type} with id {id},{id2}");
        }

        public static void AddEntityException(ILogger logger, Exception ex, string type)
        {
            logger.LogError(ex, $"Error while Inserting new {type}");
        }
    }
}
