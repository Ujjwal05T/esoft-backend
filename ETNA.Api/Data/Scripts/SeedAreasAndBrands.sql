-- SQL Script to seed Areas and Brands tables with dummy data
-- Run this after CreateSalesPersonTables.sql

-- =====================================================
-- Seed Areas Table
-- =====================================================
INSERT INTO Areas (Name, State, Cities) VALUES
('Mumbai West', 'Maharashtra', '["Mumbai", "Thane", "Navi Mumbai"]'),
('Mumbai Central', 'Maharashtra', '["Mumbai"]'),
('Pune Region', 'Maharashtra', '["Pune", "Pimpri-Chinchwad"]'),
('Indore Zone', 'Madhya Pradesh', '["Indore", "Dewas"]'),
('Bhopal Region', 'Madhya Pradesh', '["Bhopal", "Vidisha"]'),
('Nagpur Zone', 'Maharashtra', '["Nagpur", "Wardha"]'),
('Ahmedabad Region', 'Gujarat', '["Ahmedabad", "Gandhinagar"]'),
('Surat Zone', 'Gujarat', '["Surat", "Bharuch"]'),
('Vadodara Region', 'Gujarat', '["Vadodara", "Anand"]'),
('Jaipur Region', 'Rajasthan', '["Jaipur", "Ajmer"]'),
('Jodhpur Zone', 'Rajasthan', '["Jodhpur", "Bikaner"]'),
('Delhi NCR', 'Delhi', '["Delhi", "Noida", "Gurgaon", "Faridabad"]'),
('Bangalore Urban', 'Karnataka', '["Bangalore"]'),
('Bangalore Rural', 'Karnataka', '["Bangalore", "Mysuru"]'),
('Chennai Region', 'Tamil Nadu', '["Chennai", "Kanchipuram"]'),
('Hyderabad Zone', 'Telangana', '["Hyderabad", "Secunderabad"]'),
('Kolkata Metro', 'West Bengal', '["Kolkata", "Howrah"]'),
('Lucknow Region', 'Uttar Pradesh', '["Lucknow", "Kanpur"]');

PRINT 'Areas seeded successfully with 18 records!';

-- =====================================================
-- Seed Brands Table
-- =====================================================
INSERT INTO Brands (Name, IsActive) VALUES
('Maruti Suzuki', 1),
('Hyundai', 1),
('Tata', 1),
('Mahindra', 1),
('Honda', 1),
('Toyota', 1),
('Kia', 1),
('MG', 1),
('Volkswagen', 1),
('Skoda', 1),
('Ford', 1),
('Renault', 1),
('Nissan', 1),
('Jeep', 1),
('BMW', 1),
('Mercedes-Benz', 1),
('Audi', 1),
('Lexus', 1),
('Volvo', 1),
('Mini', 1),
('Porsche', 1),
('Jaguar', 1),
('Land Rover', 1),
('Ferrari', 1),
('Lamborghini', 1),
('Bentley', 1),
('Rolls-Royce', 1),
('Chevrolet', 0), -- Inactive, exited India
('Fiat', 0), -- Inactive, exited India
('Datsun', 0); -- Inactive, discontinued

PRINT 'Brands seeded successfully with 30 records!';

-- =====================================================
-- Verify data
-- =====================================================
SELECT 'Areas Count: ' + CAST(COUNT(*) AS NVARCHAR(10)) FROM Areas;
SELECT 'Brands Count: ' + CAST(COUNT(*) AS NVARCHAR(10)) FROM Brands;

PRINT 'Seed data inserted successfully!';
