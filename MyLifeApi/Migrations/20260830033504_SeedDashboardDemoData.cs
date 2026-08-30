using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLifeApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedDashboardDemoData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO categories (user_id, name, description, icon, color, is_active)
                SELECT u.id, '[Demo] Bem-estar', 'Categoria criada pela migration de demonstração.', 'heart', '#70c52f', TRUE
                FROM users u
                WHERE u.status = 'active'
                  AND NOT EXISTS (
                      SELECT 1 FROM categories c
                      WHERE c.user_id = u.id AND c.name = '[Demo] Bem-estar'
                  );

                INSERT INTO accounts (user_id, name, account_type, initial_balance, is_active)
                SELECT u.id, '[Demo] Conta principal', 'checking', 850.00, TRUE
                FROM users u
                WHERE u.status = 'active'
                  AND NOT EXISTS (
                      SELECT 1 FROM accounts a
                      WHERE a.user_id = u.id AND a.name = '[Demo] Conta principal'
                  );
                """);

            var financialCategories = new[]
            {
                (Name: "[Demo] Receitas", Type: "income", Icon: "wallet", Color: "#70c52f"),
                (Name: "[Demo] Alimentação", Type: "expense", Icon: "utensils", Color: "#ff8a65"),
                (Name: "[Demo] Transporte", Type: "expense", Icon: "car", Color: "#5c8df6"),
                (Name: "[Demo] Desenvolvimento", Type: "expense", Icon: "book", Color: "#9b7af7"),
                (Name: "[Demo] Assinaturas", Type: "expense", Icon: "repeat", Color: "#f5b942")
            };

            foreach (var category in financialCategories)
            {
                migrationBuilder.Sql(
                    $"""
                    INSERT INTO financial_categories (user_id, name, category_type, icon, color, is_active)
                    SELECT u.id, '{category.Name}', '{category.Type}', '{category.Icon}', '{category.Color}', TRUE
                    FROM users u
                    WHERE u.status = 'active'
                      AND NOT EXISTS (
                          SELECT 1 FROM financial_categories fc
                          WHERE fc.user_id = u.id
                            AND fc.name = '{category.Name}'
                            AND fc.category_type = '{category.Type}'
                      );
                    """);
            }

            migrationBuilder.Sql(
                """
                INSERT INTO goals
                    (user_id, title, description, goal_type, target_value, unit, start_date, target_date, status)
                SELECT
                    u.id,
                    '[Demo] Construir uma rotina mais leve',
                    'Concluir oito atividades para criar consistência durante o mês.',
                    'quantity',
                    8.00,
                    'atividades',
                    CURRENT_DATE(),
                    DATE_ADD(CURRENT_DATE(), INTERVAL 30 DAY),
                    'active'
                FROM users u
                WHERE u.status = 'active'
                  AND NOT EXISTS (
                      SELECT 1 FROM goals g
                      WHERE g.user_id = u.id
                        AND g.title = '[Demo] Construir uma rotina mais leve'
                  );
                """);

            var completedActivities = new[]
            {
                (Title: "[Demo] Caminhada leve", DaysAgo: 6, DurationMinutes: 30),
                (Title: "[Demo] Leitura focada", DaysAgo: 5, DurationMinutes: 45),
                (Title: "[Demo] Organizar prioridades", DaysAgo: 4, DurationMinutes: 20),
                (Title: "[Demo] Estudo do projeto", DaysAgo: 3, DurationMinutes: 60),
                (Title: "[Demo] Exercício e alongamento", DaysAgo: 2, DurationMinutes: 40),
                (Title: "[Demo] Revisão da semana", DaysAgo: 1, DurationMinutes: 50),
                (Title: "[Demo] Planejamento do dia", DaysAgo: 0, DurationMinutes: 25)
            };

            foreach (var activity in completedActivities)
            {
                migrationBuilder.Sql(CreateCompletedActivitySql(
                    activity.Title,
                    activity.DaysAgo,
                    activity.DurationMinutes));
            }

            migrationBuilder.Sql(
                """
                INSERT INTO activities
                    (user_id, category_id, title, description, activity_date, started_at, ended_at,
                     duration_seconds, status, source)
                SELECT
                    u.id,
                    c.id,
                    '[Demo] Caminhada no fim do dia',
                    'Atividade futura usada para demonstrar a agenda do dashboard.',
                    CURRENT_DATE(),
                    DATE_ADD(CURRENT_DATE(), INTERVAL 18 HOUR),
                    NULL,
                    NULL,
                    'planned',
                    'manual'
                FROM users u
                INNER JOIN categories c
                    ON c.user_id = u.id AND c.name = '[Demo] Bem-estar'
                WHERE u.status = 'active'
                  AND NOT EXISTS (
                      SELECT 1 FROM activities a
                      WHERE a.user_id = u.id
                        AND a.title = '[Demo] Caminhada no fim do dia'
                  );

                INSERT INTO activity_goals (activity_id, goal_id)
                SELECT a.id, g.id
                FROM activities a
                INNER JOIN goals g
                    ON g.user_id = a.user_id
                   AND g.title = '[Demo] Construir uma rotina mais leve'
                WHERE a.title LIKE '[Demo] %'
                  AND a.status = 'completed'
                  AND NOT EXISTS (
                      SELECT 1 FROM activity_goals ag
                      WHERE ag.activity_id = a.id AND ag.goal_id = g.id
                  );
                """);

            var transactions = new[]
            {
                (Description: "[Demo] Salário", Type: "income", Category: "[Demo] Receitas", Amount: "3500.00", DaysAgo: 6),
                (Description: "[Demo] Supermercado", Type: "expense", Category: "[Demo] Alimentação", Amount: "220.00", DaysAgo: 5),
                (Description: "[Demo] Transporte", Type: "expense", Category: "[Demo] Transporte", Amount: "85.00", DaysAgo: 4),
                (Description: "[Demo] Curso online", Type: "expense", Category: "[Demo] Desenvolvimento", Amount: "149.90", DaysAgo: 3),
                (Description: "[Demo] Projeto freelance", Type: "income", Category: "[Demo] Receitas", Amount: "620.00", DaysAgo: 2),
                (Description: "[Demo] Jantar", Type: "expense", Category: "[Demo] Alimentação", Amount: "96.50", DaysAgo: 1),
                (Description: "[Demo] Assinatura", Type: "expense", Category: "[Demo] Assinaturas", Amount: "39.90", DaysAgo: 0)
            };

            foreach (var transaction in transactions)
            {
                migrationBuilder.Sql(CreateTransactionSql(
                    transaction.Description,
                    transaction.Type,
                    transaction.Category,
                    transaction.Amount,
                    transaction.DaysAgo));
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE ag
                FROM activity_goals ag
                INNER JOIN activities a ON a.id = ag.activity_id
                INNER JOIN goals g ON g.id = ag.goal_id
                WHERE a.title LIKE '[Demo] %'
                   OR g.title = '[Demo] Construir uma rotina mais leve';

                DELETE FROM transactions WHERE description LIKE '[Demo] %';
                DELETE FROM activities WHERE title LIKE '[Demo] %';
                DELETE FROM goals WHERE title = '[Demo] Construir uma rotina mais leve';
                DELETE FROM financial_categories WHERE name LIKE '[Demo] %';
                DELETE FROM accounts WHERE name = '[Demo] Conta principal';
                DELETE FROM categories WHERE name = '[Demo] Bem-estar';
                """);
        }

        private static string CreateCompletedActivitySql(
            string title,
            int daysAgo,
            int durationMinutes)
        {
            var activityDate = daysAgo == 0
                ? "CURRENT_DATE()"
                : $"DATE_SUB(CURRENT_DATE(), INTERVAL {daysAgo} DAY)";
            var durationSeconds = durationMinutes * 60;

            return
                $"""
                INSERT INTO activities
                    (user_id, category_id, title, description, activity_date, started_at, ended_at,
                     duration_seconds, status, source)
                SELECT
                    u.id,
                    c.id,
                    '{title}',
                    'Atividade criada pela migration de demonstração.',
                    {activityDate},
                    DATE_ADD({activityDate}, INTERVAL 9 HOUR),
                    DATE_ADD(DATE_ADD({activityDate}, INTERVAL 9 HOUR), INTERVAL {durationMinutes} MINUTE),
                    {durationSeconds},
                    'completed',
                    'manual'
                FROM users u
                INNER JOIN categories c
                    ON c.user_id = u.id AND c.name = '[Demo] Bem-estar'
                WHERE u.status = 'active'
                  AND NOT EXISTS (
                      SELECT 1 FROM activities a
                      WHERE a.user_id = u.id AND a.title = '{title}'
                  );
                """;
        }

        private static string CreateTransactionSql(
            string description,
            string transactionType,
            string categoryName,
            string amount,
            int daysAgo)
        {
            var transactionDate = daysAgo == 0
                ? "CURRENT_DATE()"
                : $"DATE_SUB(CURRENT_DATE(), INTERVAL {daysAgo} DAY)";

            return
                $"""
                INSERT INTO transactions
                    (user_id, account_id, category_id, recurring_transaction_id, transaction_type,
                     description, amount, transaction_date, status, notes)
                SELECT
                    u.id,
                    a.id,
                    fc.id,
                    NULL,
                    '{transactionType}',
                    '{description}',
                    {amount},
                    {transactionDate},
                    'completed',
                    'Lançamento criado pela migration de demonstração.'
                FROM users u
                INNER JOIN accounts a
                    ON a.user_id = u.id AND a.name = '[Demo] Conta principal'
                INNER JOIN financial_categories fc
                    ON fc.user_id = u.id
                   AND fc.name = '{categoryName}'
                   AND fc.category_type = '{transactionType}'
                WHERE u.status = 'active'
                  AND NOT EXISTS (
                      SELECT 1 FROM transactions t
                      WHERE t.user_id = u.id AND t.description = '{description}'
                  );
                """;
        }
    }
}
