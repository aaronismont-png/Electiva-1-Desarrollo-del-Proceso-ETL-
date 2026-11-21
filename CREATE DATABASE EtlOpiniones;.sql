CREATE DATABASE EtlOpiniones;
GO
USE EtlOpiniones;
GO
CREATE TABLE Reviews (
    ReviewId    INT IDENTITY(1,1) PRIMARY KEY,
    ProductId   INT NOT NULL,
    CustomerId  INT NOT NULL,
    Rating      INT NOT NULL,
    ReviewText  NVARCHAR(500) NULL,
    ReviewDate  DATETIME NOT NULL
);
GO
INSERT INTO Reviews (ProductId, CustomerId, Rating, ReviewText, ReviewDate) VALUES
(1, 101, 5, N'Excelente producto, muy recomendado', '2026-10-01'),
(1, 102, 4, N'Buena calidad, llegó a tiempo', '2026-10-02'),
(2, 103, 2, N'No cumplió mis expectativas', '2026-10-03'),
(3, 104, 5, N'Superó lo que esperaba', '2026-10-04'),
(2, 105, 3, N'Regular, podría mejorar', '2026-10-05');
GO