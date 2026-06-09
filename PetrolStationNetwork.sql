-- phpMyAdmin SQL Dump
-- version 5.2.0
-- https://www.phpmyadmin.net/
--
-- Хост: 127.0.0.1:3306
-- Время создания: Июн 09 2026 г., 10:26
-- Версия сервера: 5.7.39-log
-- Версия PHP: 8.1.9

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- База данных: `PetrolStationNetwork`
--

-- --------------------------------------------------------

--
-- Структура таблицы `deliveries`
--

CREATE TABLE `deliveries` (
  `id` int(11) NOT NULL COMMENT 'Идентификатор',
  `supplier_id` int(11) DEFAULT NULL COMMENT 'Идентификатор поставщика	',
  `serial_number` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Серия и номер',
  `date` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT 'Дата',
  `status` enum('В ожидании','В обработке','Принята','Отменена') COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Статус'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Поставки';

-- --------------------------------------------------------

--
-- Структура таблицы `DeliveryItems`
--

CREATE TABLE `DeliveryItems` (
  `id` int(11) NOT NULL COMMENT 'Идентификатор',
  `delivery_id` int(11) NOT NULL COMMENT 'Идентификатор поставки',
  `product_id` int(11) NOT NULL COMMENT 'Идентификатор продуктов',
  `count` int(11) NOT NULL COMMENT 'Количество',
  `exp_date` date NOT NULL COMMENT 'Срок годности'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Список позиций поставки';

-- --------------------------------------------------------

--
-- Структура таблицы `Products`
--

CREATE TABLE `Products` (
  `id` int(11) NOT NULL COMMENT 'Идентификатор',
  `name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Название'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Список товаров';

-- --------------------------------------------------------

--
-- Структура таблицы `ShopItems`
--

CREATE TABLE `ShopItems` (
  `id` int(11) NOT NULL COMMENT 'Идентификатор',
  `warehouse_item_id` int(11) DEFAULT NULL COMMENT 'Идентификатор позиции на складе',
  `count` int(11) DEFAULT NULL COMMENT 'Количество на продаже'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Товары на продаже';

-- --------------------------------------------------------

--
-- Структура таблицы `Staff`
--

CREATE TABLE `Staff` (
  `user_id` int(11) NOT NULL COMMENT 'Идентификатор пользовтеля',
  `role` enum('leader','worker') COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Роль'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Список сотрудников';

--
-- Дамп данных таблицы `Staff`
--

INSERT INTO `Staff` (`user_id`, `role`) VALUES
(5, 'leader');

-- --------------------------------------------------------

--
-- Структура таблицы `Suppliers`
--

CREATE TABLE `Suppliers` (
  `user_id` int(11) NOT NULL COMMENT 'Идентификатор пользователя',
  `company_name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Название компании'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Поставщики';

--
-- Дамп данных таблицы `Suppliers`
--

INSERT INTO `Suppliers` (`user_id`, `company_name`) VALUES
(5, 'ADMIN');

-- --------------------------------------------------------

--
-- Структура таблицы `Users`
--

CREATE TABLE `Users` (
  `id` int(11) NOT NULL COMMENT 'Идентификатор',
  `full_name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'ФИО',
  `tel_number` varchar(15) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Номер телефона',
  `login` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Логин',
  `password` varchar(30) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Пароль'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Пользователи';

--
-- Дамп данных таблицы `Users`
--

INSERT INTO `Users` (`id`, `full_name`, `tel_number`, `login`, `password`) VALUES
(5, 'admin', 'admin', 'admin', 'admin');

-- --------------------------------------------------------

--
-- Структура таблицы `WarehouseItems`
--

CREATE TABLE `WarehouseItems` (
  `id` int(11) NOT NULL COMMENT 'Идентификатор',
  `delivery_items_id` int(11) DEFAULT NULL COMMENT 'Идентификатор позиций поставки',
  `product_id` int(11) NOT NULL COMMENT 'Идентификатор товара',
  `count` int(11) NOT NULL COMMENT 'Количество на складе',
  `exp_date` date NOT NULL COMMENT 'Срок годности',
  `position` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Позиция на складе'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci COMMENT='Товары на складе';

--
-- Индексы сохранённых таблиц
--

--
-- Индексы таблицы `deliveries`
--
ALTER TABLE `deliveries`
  ADD PRIMARY KEY (`id`),
  ADD KEY `supplier_id` (`supplier_id`);

--
-- Индексы таблицы `DeliveryItems`
--
ALTER TABLE `DeliveryItems`
  ADD PRIMARY KEY (`id`),
  ADD KEY `delivery_id` (`delivery_id`),
  ADD KEY `product_id` (`product_id`);

--
-- Индексы таблицы `Products`
--
ALTER TABLE `Products`
  ADD PRIMARY KEY (`id`);

--
-- Индексы таблицы `ShopItems`
--
ALTER TABLE `ShopItems`
  ADD PRIMARY KEY (`id`),
  ADD KEY `warehouse_item_id` (`warehouse_item_id`);

--
-- Индексы таблицы `Staff`
--
ALTER TABLE `Staff`
  ADD PRIMARY KEY (`user_id`);

--
-- Индексы таблицы `Suppliers`
--
ALTER TABLE `Suppliers`
  ADD PRIMARY KEY (`user_id`);

--
-- Индексы таблицы `Users`
--
ALTER TABLE `Users`
  ADD PRIMARY KEY (`id`);

--
-- Индексы таблицы `WarehouseItems`
--
ALTER TABLE `WarehouseItems`
  ADD PRIMARY KEY (`id`),
  ADD KEY `product_id` (`product_id`),
  ADD KEY `delivery_items_id` (`delivery_items_id`);

--
-- AUTO_INCREMENT для сохранённых таблиц
--

--
-- AUTO_INCREMENT для таблицы `deliveries`
--
ALTER TABLE `deliveries`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Идентификатор', AUTO_INCREMENT=16;

--
-- AUTO_INCREMENT для таблицы `DeliveryItems`
--
ALTER TABLE `DeliveryItems`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Идентификатор', AUTO_INCREMENT=15;

--
-- AUTO_INCREMENT для таблицы `Products`
--
ALTER TABLE `Products`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Идентификатор', AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT для таблицы `ShopItems`
--
ALTER TABLE `ShopItems`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Идентификатор', AUTO_INCREMENT=9;

--
-- AUTO_INCREMENT для таблицы `Users`
--
ALTER TABLE `Users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Идентификатор', AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT для таблицы `WarehouseItems`
--
ALTER TABLE `WarehouseItems`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT COMMENT 'Идентификатор', AUTO_INCREMENT=10;

--
-- Ограничения внешнего ключа сохраненных таблиц
--

--
-- Ограничения внешнего ключа таблицы `deliveries`
--
ALTER TABLE `deliveries`
  ADD CONSTRAINT `deliveries_ibfk_1` FOREIGN KEY (`supplier_id`) REFERENCES `Suppliers` (`user_id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `DeliveryItems`
--
ALTER TABLE `DeliveryItems`
  ADD CONSTRAINT `deliveryitems_ibfk_1` FOREIGN KEY (`delivery_id`) REFERENCES `deliveries` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `deliveryitems_ibfk_2` FOREIGN KEY (`product_id`) REFERENCES `Products` (`id`) ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `ShopItems`
--
ALTER TABLE `ShopItems`
  ADD CONSTRAINT `shopitems_ibfk_1` FOREIGN KEY (`warehouse_item_id`) REFERENCES `WarehouseItems` (`id`) ON DELETE SET NULL ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `Staff`
--
ALTER TABLE `Staff`
  ADD CONSTRAINT `staff_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `Suppliers`
--
ALTER TABLE `Suppliers`
  ADD CONSTRAINT `suppliers_ibfk_1` FOREIGN KEY (`user_id`) REFERENCES `Users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Ограничения внешнего ключа таблицы `WarehouseItems`
--
ALTER TABLE `WarehouseItems`
  ADD CONSTRAINT `warehouseitems_ibfk_2` FOREIGN KEY (`delivery_items_id`) REFERENCES `DeliveryItems` (`id`) ON DELETE SET NULL ON UPDATE CASCADE,
  ADD CONSTRAINT `warehouseitems_ibfk_3` FOREIGN KEY (`product_id`) REFERENCES `Products` (`id`) ON UPDATE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
