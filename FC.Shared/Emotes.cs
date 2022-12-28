﻿// Copyright (c) FCChan. All rights reserved.
//
// Licensed under the MIT license.

namespace FC
{
	using System;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Text;

	public class Emotes
	{
		public const string Hash = "#";
		public const string Zero = "0";
		public const string One = "1";
		public const string Two = "2";
		public const string Three = "3";
		public const string Four = "4";
		public const string Five = "5";
		public const string Six = "6";
		public const string Seven = "7";
		public const string Eight = "8";
		public const string Nine = "9";
		public const string Copyright = "©";
		public const string Registered = "®";
		public const string Bangbang = "‼";
		public const string Interrobang = "⁉";
		public const string Tm = "™";
		public const string InformationSource = "ℹ";
		public const string LeftRightArrow = "↔";
		public const string ArrowUpDown = "↕";
		public const string ArrowUpperLeft = "↖";
		public const string ArrowUpperRight = "↗";
		public const string ArrowLowerRight = "↘";
		public const string ArrowLowerLeft = "↙";
		public const string LeftwardsArrowWithHook = "↩";
		public const string ArrowRightHook = "↪";
		public const string Watch = "⌚";
		public const string Hourglass = "⌛";
		public const string FastForward = "⏩";
		public const string Rewind = "⏪";
		public const string ArrowDoubleUp = "⏫";
		public const string ArrowDoubleDown = "⏬";
		public const string AlarmClock = "⏰";
		public const string HourglassFlowingSand = "⏳";
		public const string M = "Ⓜ";
		public const string BlackSmallSquare = "▪";
		public const string WhiteSmallSquare = "▫";
		public const string ArrowForward = "▶";
		public const string ArrowBackward = "◀";
		public const string WhiteMediumSquare = "◻";
		public const string BlackMediumSquare = "◼";
		public const string WhiteMediumSmallSquare = "◽";
		public const string BlackMediumSmallSquare = "◾";
		public const string Sunny = "☀";
		public const string Cloud = "☁";
		public const string Telephone = "☎";
		public const string BallotBoxWithCheck = "☑";
		public const string Umbrella = "☔";
		public const string Coffee = "☕";
		public const string PointUp = "☝";
		public const string Relaxed = "☺";
		public const string Aries = "♈";
		public const string Taurus = "♉";
		public const string Gemini = "♊";
		public const string Cancer = "♋";
		public const string Leo = "♌";
		public const string Virgo = "♍";
		public const string Libra = "♎";
		public const string Scorpius = "♏";
		public const string Sagittarius = "♐";
		public const string Capricorn = "♑";
		public const string Aquarius = "♒";
		public const string Pisces = "♓";
		public const string Spades = "♠";
		public const string Clubs = "♣";
		public const string Hearts = "♥";
		public const string Diamonds = "♦";
		public const string Hotsprings = "♨";
		public const string Recycle = "♻";
		public const string Wheelchair = "♿";
		public const string Anchor = "⚓";
		public const string Warning = "⚠";
		public const string Zap = "⚡";
		public const string WhiteCircle = "⚪";
		public const string BlackCircle = "⚫";
		public const string Soccer = "⚽";
		public const string Baseball = "⚾";
		public const string Snowman = "⛄";
		public const string PartlySunny = "⛅";
		public const string Ophiuchus = "⛎";
		public const string NoEntry = "⛔";
		public const string Church = "⛪";
		public const string Fountain = "⛲";
		public const string Golf = "⛳";
		public const string Sailboat = "⛵";
		public const string Tent = "⛺";
		public const string Fuelpump = "⛽";
		public const string Scissors = "✂";
		public const string WhiteCheckMark = "✅";
		public const string Airplane = "✈";
		public const string Envelope = "✉";
		public const string Fist = "✊";
		public const string RaisedHand = "✋";
		public const string V = "✌";
		public const string Pencil2 = "✏";
		public const string BlackNib = "✒";
		public const string HeavyCheckMark = "✔";
		public const string HeavyMultiplicationX = "✖";
		public const string Sparkles = "✨";
		public const string EightSpokedAsterisk = "✳";
		public const string EightPointedBlackStar = "✴";
		public const string Snowflake = "❄";
		public const string Sparkle = "❇";
		public const string X = "❌";
		public const string NegativeSquaredCrossMark = "❎";
		public const string Question = "❓";
		public const string GreyQuestion = "❔";
		public const string GreyExclamation = "❕";
		public const string Exclamation = "❗";
		public const string Heart = "❤";
		public const string HeavyPlusSign = "➕";
		public const string HeavyMinusSign = "➖";
		public const string HeavyDivisionSign = "➗";
		public const string ArrowRight = "➡";
		public const string CurlyLoop = "➰";
		public const string ArrowHeadingUp = "⤴";
		public const string ArrowHeadingDown = "⤵";
		public const string ArrowLeft = "⬅";
		public const string ArrowUp = "⬆";
		public const string ArrowDown = "⬇";
		public const string BlackLargeSquare = "⬛";
		public const string WhiteLargeSquare = "⬜";
		public const string Star = "⭐";
		public const string O = "⭕";
		public const string WavyDash = "〰";
		public const string PartAlternationMark = "〽";
		public const string Congratulations = "㊗";
		public const string Secret = "㊙";
		public const string Mahjong = "🀄";
		public const string BlackJoker = "🃏";
		public const string A = "🅰";
		public const string B = "🅱";
		public const string O2 = "🅾";
		public const string Parking = "🅿";
		public const string Ab = "🆎";
		public const string Cl = "🆑";
		public const string Cool = "🆒";
		public const string Free = "🆓";
		public const string Id = "🆔";
		public const string New = "🆕";
		public const string Ng = "🆖";
		public const string Ok = "🆗";
		public const string Sos = "🆘";
		public const string Up = "🆙";
		public const string Vs = "🆚";
		public const string Cn = "🇨 🇳";
		public const string De = "🇩 🇪";
		public const string Es = "🇪 🇸";
		public const string Fr = "🇫 🇷";
		public const string Uk = "🇬 🇧";
		public const string It = "🇮 🇹";
		public const string Jp = "🇯 🇵";
		public const string Kr = "🇰 🇷";
		public const string Ru = "🇷 🇺";
		public const string Us = "🇺 🇸";
		public const string Koko = "🈁";
		public const string Sa = "🈂";
		public const string U7121 = "🈚";
		public const string U6307 = "🈯";
		public const string U7981 = "🈲";
		public const string U7A7A = "🈳";
		public const string U5408 = "🈴";
		public const string U6E80 = "🈵";
		public const string U6709 = "🈶";
		public const string U6708 = "🈷";
		public const string U7533 = "🈸";
		public const string U5272 = "🈹";
		public const string U55B6 = "🈺";
		public const string IdeographAdvantage = "🉐";
		public const string Accept = "🉑";
		public const string Cyclone = "🌀";
		public const string Foggy = "🌁";
		public const string ClosedUmbrella = "🌂";
		public const string NightWithStars = "🌃";
		public const string SunriseOverMountains = "🌄";
		public const string Sunrise = "🌅";
		public const string CitySunset = "🌆";
		public const string CitySunrise = "🌇";
		public const string Rainbow = "🌈";
		public const string BridgeAtNight = "🌉";
		public const string Ocean = "🌊";
		public const string Volcano = "🌋";
		public const string MilkyWay = "🌌";
		public const string EarthAsia = "🌏";
		public const string NewMoon = "🌑";
		public const string FirstQuarterMoon = "🌓";
		public const string WaxingGibbousMoon = "🌔";
		public const string FullMoon = "🌕";
		public const string CrescentMoon = "🌙";
		public const string FirstQuarterMoonWithFace = "🌛";
		public const string Star2 = "🌟";
		public const string Stars = "🌠";
		public const string Chestnut = "🌰";
		public const string Seedling = "🌱";
		public const string PalmTree = "🌴";
		public const string Cactus = "🌵";
		public const string Tulip = "🌷";
		public const string CherryBlossom = "🌸";
		public const string Rose = "🌹";
		public const string Hibiscus = "🌺";
		public const string Sunflower = "🌻";
		public const string Blossom = "🌼";
		public const string Corn = "🌽";
		public const string EarOfRice = "🌾";
		public const string Herb = "🌿";
		public const string FourLeafClover = "🍀";
		public const string MapleLeaf = "🍁";
		public const string FallenLeaf = "🍂";
		public const string Leaves = "🍃";
		public const string Mushroom = "🍄";
		public const string Tomato = "🍅";
		public const string Eggplant = "🍆";
		public const string Grapes = "🍇";
		public const string Melon = "🍈";
		public const string Watermelon = "🍉";
		public const string Tangerine = "🍊";
		public const string Banana = "🍌";
		public const string Pineapple = "🍍";
		public const string Apple = "🍎";
		public const string GreenApple = "🍏";
		public const string Peach = "🍑";
		public const string Cherries = "🍒";
		public const string Strawberry = "🍓";
		public const string Hamburger = "🍔";
		public const string Pizza = "🍕";
		public const string MeatOnBone = "🍖";
		public const string PoultryLeg = "🍗";
		public const string RiceCracker = "🍘";
		public const string RiceBall = "🍙";
		public const string Rice = "🍚";
		public const string Curry = "🍛";
		public const string Ramen = "🍜";
		public const string Spaghetti = "🍝";
		public const string Bread = "🍞";
		public const string Fries = "🍟";
		public const string SweetPotato = "🍠";
		public const string Dango = "🍡";
		public const string Oden = "🍢";
		public const string Sushi = "🍣";
		public const string FriedShrimp = "🍤";
		public const string FishCake = "🍥";
		public const string Icecream = "🍦";
		public const string ShavedIce = "🍧";
		public const string IceCream = "🍨";
		public const string Doughnut = "🍩";
		public const string Cookie = "🍪";
		public const string ChocolateBar = "🍫";
		public const string Candy = "🍬";
		public const string Lollipop = "🍭";
		public const string Custard = "🍮";
		public const string HoneyPot = "🍯";
		public const string Cake = "🍰";
		public const string Bento = "🍱";
		public const string Stew = "🍲";
		public const string Egg = "🍳";
		public const string ForkAndKnife = "🍴";
		public const string Tea = "🍵";
		public const string Sake = "🍶";
		public const string WineGlass = "🍷";
		public const string Cocktail = "🍸";
		public const string TropicalDrink = "🍹";
		public const string Beer = "🍺";
		public const string Beers = "🍻";
		public const string Ribbon = "🎀";
		public const string Gift = "🎁";
		public const string Birthday = "🎂";
		public const string JackOLantern = "🎃";
		public const string ChristmasTree = "🎄";
		public const string Santa = "🎅";
		public const string Fireworks = "🎆";
		public const string Sparkler = "🎇";
		public const string Balloon = "🎈";
		public const string Tada = "🎉";
		public const string ConfettiBall = "🎊";
		public const string TanabataTree = "🎋";
		public const string CrossedFlags = "🎌";
		public const string Bamboo = "🎍";
		public const string Dolls = "🎎";
		public const string Flags = "🎏";
		public const string WindChime = "🎐";
		public const string RiceScene = "🎑";
		public const string SchoolSatchel = "🎒";
		public const string MortarBoard = "🎓";
		public const string CarouselHorse = "🎠";
		public const string FerrisWheel = "🎡";
		public const string RollerCoaster = "🎢";
		public const string FishingPoleAndFish = "🎣";
		public const string Microphone = "🎤";
		public const string MovieCamera = "🎥";
		public const string Cinema = "🎦";
		public const string Headphones = "🎧";
		public const string Art = "🎨";
		public const string Tophat = "🎩";
		public const string CircusTent = "🎪";
		public const string Ticket = "🎫";
		public const string Clapper = "🎬";
		public const string PerformingArts = "🎭";
		public const string VideoGame = "🎮";
		public const string Dart = "🎯";
		public const string SlotMachine = "🎰";
		public const string EightBall = "🎱";
		public const string GameDie = "🎲";
		public const string Bowling = "🎳";
		public const string FlowerPlayingCards = "🎴";
		public const string MusicalNote = "🎵";
		public const string Notes = "🎶";
		public const string Saxophone = "🎷";
		public const string Guitar = "🎸";
		public const string MusicalKeyboard = "🎹";
		public const string Trumpet = "🎺";
		public const string Violin = "🎻";
		public const string MusicalScore = "🎼";
		public const string RunningShirtWithSash = "🎽";
		public const string Tennis = "🎾";
		public const string Ski = "🎿";
		public const string Basketball = "🏀";
		public const string CheckeredFlag = "🏁";
		public const string Snowboarder = "🏂";
		public const string Runner = "🏃";
		public const string Surfer = "🏄";
		public const string Trophy = "🏆";
		public const string Football = "🏈";
		public const string Swimmer = "🏊";
		public const string House = "🏠";
		public const string HouseWithGarden = "🏡";
		public const string Office = "🏢";
		public const string PostOffice = "🏣";
		public const string Hospital = "🏥";
		public const string Bank = "🏦";
		public const string Atm = "🏧";
		public const string Hotel = "🏨";
		public const string LoveHotel = "🏩";
		public const string ConvenienceStore = "🏪";
		public const string School = "🏫";
		public const string DepartmentStore = "🏬";
		public const string Factory = "🏭";
		public const string IzakayaLantern = "🏮";
		public const string JapaneseCastle = "🏯";
		public const string EuropeanCastle = "🏰";
		public const string Snail = "🐌";
		public const string Snake = "🐍";
		public const string Racehorse = "🐎";
		public const string Sheep = "🐑";
		public const string Monkey = "🐒";
		public const string Chicken = "🐔";
		public const string Boar = "🐗";
		public const string Elephant = "🐘";
		public const string Octopus = "🐙";
		public const string Shell = "🐚";
		public const string Bug = "🐛";
		public const string Ant = "🐜";
		public const string Bee = "🐝";
		public const string Beetle = "🐞";
		public const string Fish = "🐟";
		public const string TropicalFish = "🐠";
		public const string Blowfish = "🐡";
		public const string Turtle = "🐢";
		public const string HatchingChick = "🐣";
		public const string BabyChick = "🐤";
		public const string HatchedChick = "🐥";
		public const string Bird = "🐦";
		public const string Penguin = "🐧";
		public const string Koala = "🐨";
		public const string Poodle = "🐩";
		public const string Camel = "🐫";
		public const string Dolphin = "🐬";
		public const string Mouse = "🐭";
		public const string Cow = "🐮";
		public const string Tiger = "🐯";
		public const string Rabbit = "🐰";
		public const string Cat = "🐱";
		public const string DragonFace = "🐲";
		public const string Whale = "🐳";
		public const string Horse = "🐴";
		public const string MonkeyFace = "🐵";
		public const string Dog = "🐶";
		public const string Pig = "🐷";
		public const string Frog = "🐸";
		public const string Hamster = "🐹";
		public const string Wolf = "🐺";
		public const string Bear = "🐻";
		public const string PandaFace = "🐼";
		public const string PigNose = "🐽";
		public const string Feet = "🐾";
		public const string Eyes = "👀";
		public const string Ear = "👂";
		public const string Nose = "👃";
		public const string Lips = "👄";
		public const string Tongue = "👅";
		public const string PointUp2 = "👆";
		public const string PointDown = "👇";
		public const string PointLeft = "👈";
		public const string PointRight = "👉";
		public const string Punch = "👊";
		public const string Wave = "👋";
		public const string OkHand = "👌";
		public const string Thumbsup = "👍";
		public const string Thumbsdown = "👎";
		public const string Clap = "👏";
		public const string OpenHands = "👐";
		public const string Crown = "👑";
		public const string WomansHat = "👒";
		public const string Eyeglasses = "👓";
		public const string Necktie = "👔";
		public const string Shirt = "👕";
		public const string Jeans = "👖";
		public const string Dress = "👗";
		public const string Kimono = "👘";
		public const string Bikini = "👙";
		public const string WomansClothes = "👚";
		public const string Purse = "👛";
		public const string Handbag = "👜";
		public const string Pouch = "👝";
		public const string MansShoe = "👞";
		public const string AthleticShoe = "👟";
		public const string HighHeel = "👠";
		public const string Sandal = "👡";
		public const string Boot = "👢";
		public const string Footprints = "👣";
		public const string BustInSilhouette = "👤";
		public const string Boy = "👦";
		public const string Girl = "👧";
		public const string Man = "👨";
		public const string Woman = "👩";
		public const string Family = "👪";
		public const string Couple = "👫";
		public const string Cop = "👮";
		public const string Dancers = "👯";
		public const string BrideWithVeil = "👰";
		public const string PersonWithBlondHair = "👱";
		public const string ManWithGuaPiMao = "👲";
		public const string ManWithTurban = "👳";
		public const string OlderMan = "👴";
		public const string OlderWoman = "👵";
		public const string Baby = "👶";
		public const string ConstructionWorker = "👷";
		public const string Princess = "👸";
		public const string JapaneseOgre = "👹";
		public const string JapaneseGoblin = "👺";
		public const string Ghost = "👻";
		public const string Angel = "👼";
		public const string Alien = "👽";
		public const string SpaceInvader = "👾";
		public const string RobotFace = "🤖";
		public const string Imp = "👿";
		public const string Skull = "💀";
		public const string InformationDeskPerson = "💁";
		public const string Guardsman = "💂";
		public const string Dancer = "💃";
		public const string Lipstick = "💄";
		public const string NailCare = "💅";
		public const string Massage = "💆";
		public const string Haircut = "💇";
		public const string Barber = "💈";
		public const string Syringe = "💉";
		public const string Pill = "💊";
		public const string Kiss = "💋";
		public const string LoveLetter = "💌";
		public const string Ring = "💍";
		public const string Gem = "💎";
		public const string Couplekiss = "💏";
		public const string Bouquet = "💐";
		public const string CoupleWithHeart = "💑";
		public const string Wedding = "💒";
		public const string Heartbeat = "💓";
		public const string BrokenHeart = "💔";
		public const string TwoHearts = "💕";
		public const string SparklingHeart = "💖";
		public const string Heartpulse = "💗";
		public const string Cupid = "💘";
		public const string BlueHeart = "💙";
		public const string GreenHeart = "💚";
		public const string YellowHeart = "💛";
		public const string PurpleHeart = "💜";
		public const string GiftHeart = "💝";
		public const string RevolvingHearts = "💞";
		public const string HeartDecoration = "💟";
		public const string DiamondShapeWithADotInside = "💠";
		public const string Bulb = "💡";
		public const string Anger = "💢";
		public const string Bomb = "💣";
		public const string Zzz = "💤";
		public const string Boom = "💥";
		public const string SweatDrops = "💦";
		public const string Droplet = "💧";
		public const string Dash = "💨";
		public const string Poop = "💩";
		public const string Muscle = "💪";
		public const string Dizzy = "💫";
		public const string SpeechBalloon = "💬";
		public const string WhiteFlower = "💮";
		public const string OneHundred = "💯";
		public const string Moneybag = "💰";
		public const string CurrencyExchange = "💱";
		public const string HeavyDollarSign = "💲";
		public const string CreditCard = "💳";
		public const string Yen = "💴";
		public const string Dollar = "💵";
		public const string MoneyWithWings = "💸";
		public const string Chart = "💹";
		public const string Seat = "💺";
		public const string Computer = "💻";
		public const string Briefcase = "💼";
		public const string Minidisc = "💽";
		public const string FloppyDisk = "💾";
		public const string Cd = "💿";
		public const string Dvd = "📀";
		public const string FileFolder = "📁";
		public const string OpenFileFolder = "📂";
		public const string PageWithCurl = "📃";
		public const string PageFacingUp = "📄";
		public const string Date = "📅";
		public const string Calendar = "📆";
		public const string CardIndex = "📇";
		public const string ChartWithUpwardsTrend = "📈";
		public const string ChartWithDownwardsTrend = "📉";
		public const string BarChart = "📊";
		public const string Clipboard = "📋";
		public const string Pushpin = "📌";
		public const string RoundPushpin = "📍";
		public const string Paperclip = "📎";
		public const string StraightRuler = "📏";
		public const string TriangularRuler = "📐";
		public const string BookmarkTabs = "📑";
		public const string Ledger = "📒";
		public const string Notebook = "📓";
		public const string NotebookWithDecorativeCover = "📔";
		public const string ClosedBook = "📕";
		public const string Book = "📖";
		public const string GreenBook = "📗";
		public const string BlueBook = "📘";
		public const string OrangeBook = "📙";
		public const string Books = "📚";
		public const string NameBadge = "📛";
		public const string Scroll = "📜";
		public const string Pencil = "📝";
		public const string TelephoneReceiver = "📞";
		public const string Pager = "📟";
		public const string Fax = "📠";
		public const string Satellite = "📡";
		public const string Loudspeaker = "📢";
		public const string Mega = "📣";
		public const string OutboxTray = "📤";
		public const string InboxTray = "📥";
		public const string Package = "📦";
		public const string EMail = "📧";
		public const string IncomingEnvelope = "📨";
		public const string EnvelopeWithArrow = "📩";
		public const string MailboxClosed = "📪";
		public const string Mailbox = "📫";
		public const string Postbox = "📮";
		public const string Newspaper = "📰";
		public const string Iphone = "📱";
		public const string Calling = "📲";
		public const string VibrationMode = "📳";
		public const string MobilePhoneOff = "📴";
		public const string SignalStrength = "📶";
		public const string Camera = "📷";
		public const string VideoCamera = "📹";
		public const string Tv = "📺";
		public const string Radio = "📻";
		public const string Vhs = "📼";
		public const string ArrowsClockwise = "🔃";
		public const string LoudSound = "🔊";
		public const string Battery = "🔋";
		public const string ElectricPlug = "🔌";
		public const string Mag = "🔍";
		public const string MagRight = "🔎";
		public const string LockWithInkPen = "🔏";
		public const string ClosedLockWithKey = "🔐";
		public const string Key = "🔑";
		public const string Lock = "🔒";
		public const string Unlock = "🔓";
		////public const string Bell = "🔔";
		public const string Bookmark = "🔖";
		public const string Link = "🔗";
		public const string RadioButton = "🔘";
		public const string Back = "🔙";
		public const string End = "🔚";
		public const string On = "🔛";
		public const string Soon = "🔜";
		public const string Top = "🔝";
		public const string Underage = "🔞";
		public const string KeycapTen = "🔟";
		public const string CapitalAbcd = "🔠";
		public const string Abcd = "🔡";
		public const string OneTwoThreeFour = "🔢";
		public const string Symbols = "🔣";
		public const string Abc = "🔤";
		public const string Fire = "🔥";
		public const string Flashlight = "🔦";
		public const string Wrench = "🔧";
		public const string Hammer = "🔨";
		public const string NutAndBolt = "🔩";
		public const string Knife = "🔪";
		public const string Gun = "🔫";
		public const string CrystalBall = "🔮";
		public const string SixPointedStar = "🔯";
		public const string Beginner = "🔰";
		public const string Trident = "🔱";
		public const string BlackSquareButton = "🔲";
		public const string WhiteSquareButton = "🔳";
		public const string RedCircle = "🔴";
		public const string LargeBlueCircle = "🔵";
		public const string LargeOrangeDiamond = "🔶";
		public const string LargeBlueDiamond = "🔷";
		public const string SmallOrangeDiamond = "🔸";
		public const string SmallBlueDiamond = "🔹";
		public const string SmallRedTriangle = "🔺";
		public const string SmallRedTriangleDown = "🔻";
		public const string ArrowUpSmall = "🔼";
		public const string ArrowDownSmall = "🔽";
		public const string Clock1 = "🕐";
		public const string Clock2 = "🕑";
		public const string Clock3 = "🕒";
		public const string Clock4 = "🕓";
		public const string Clock5 = "🕔";
		public const string Clock6 = "🕕";
		public const string Clock7 = "🕖";
		public const string Clock8 = "🕗";
		public const string Clock9 = "🕘";
		public const string Clock10 = "🕙";
		public const string Clock11 = "🕚";
		public const string Clock12 = "🕛";
		public const string MountFuji = "🗻";
		public const string TokyoTower = "🗼";
		public const string StatueOfLiberty = "🗽";
		public const string Japan = "🗾";
		public const string Moyai = "🗿";
		public const string Grin = "😁";
		public const string Joy = "😂";
		public const string Smiley = "😃";
		public const string Smile = "😄";
		public const string SweatSmile = "😅";
		public const string Laughing = "😆";
		public const string Wink = "😉";
		public const string Blush = "😊";
		public const string Yum = "😋";
		public const string Relieved = "😌";
		public const string HeartEyes = "😍";
		public const string Smirk = "😏";
		public const string Unamused = "😒";
		public const string Sweat = "😓";
		public const string Pensive = "😔";
		public const string Confounded = "😖";
		public const string KissingHeart = "😘";
		public const string KissingClosedEyes = "😚";
		public const string StuckOutTongueWinkingEye = "😜";
		public const string StuckOutTongueClosedEyes = "😝";
		public const string Disappointed = "😞";
		public const string Angry = "😠";
		public const string Rage = "😡";
		public const string Cry = "😢";
		public const string Persevere = "😣";
		public const string Triumph = "😤";
		public const string DisappointedRelieved = "😥";
		public const string Fearful = "😨";
		public const string Weary = "😩";
		public const string Sleepy = "😪";
		public const string TiredFace = "😫";
		public const string Sob = "😭";
		public const string ColdSweat = "😰";
		public const string Scream = "😱";
		public const string Astonished = "😲";
		public const string Flushed = "😳";
		public const string DizzyFace = "😵";
		public const string Mask = "😷";
		public const string SmileCat = "😸";
		public const string JoyCat = "😹";
		public const string SmileyCat = "😺";
		public const string HeartEyesCat = "😻";
		public const string SmirkCat = "😼";
		public const string KissingCat = "😽";
		public const string PoutingCat = "😾";
		public const string CryingCatFace = "😿";
		public const string ScreamCat = "🙀";
		public const string NoGood = "🙅";
		public const string OkWoman = "🙆";
		public const string Bow = "🙇";
		public const string SeeNoEvil = "🙈";
		public const string HearNoEvil = "🙉";
		public const string SpeakNoEvil = "🙊";
		public const string RaisingHand = "🙋";
		public const string RaisedHands = "🙌";
		public const string PersonFrowning = "🙍";
		public const string PersonWithPoutingFace = "🙎";
		public const string Pray = "🙏";
		public const string Rocket = "🚀";
		public const string RailwayCar = "🚃";
		public const string BullettrainSide = "🚄";
		public const string BullettrainFront = "🚅";
		public const string Metro = "🚇";
		public const string Station = "🚉";
		public const string Bus = "🚌";
		public const string Busstop = "🚏";
		public const string Ambulance = "🚑";
		public const string FireEngine = "🚒";
		public const string PoliceCar = "🚓";
		public const string Taxi = "🚕";
		public const string RedCar = "🚗";
		public const string BlueCar = "🚙";
		public const string Truck = "🚚";
		public const string Ship = "🚢";
		public const string Speedboat = "🚤";
		public const string TrafficLight = "🚥";
		public const string Construction = "🚧";
		public const string RotatingLight = "🚨";
		public const string TriangularFlagOnPost = "🚩";
		public const string Door = "🚪";
		public const string NoEntrySign = "🚫";
		public const string Smoking = "🚬";
		public const string NoSmoking = "🚭";
		public const string Bike = "🚲";
		public const string Walking = "🚶";
		public const string Mens = "🚹";
		public const string Womens = "🚺";
		public const string Restroom = "🚻";
		public const string BabySymbol = "🚼";
		public const string Toilet = "🚽";
		public const string Wc = "🚾";
		public const string Bath = "🛀";
		public const string ArticulatedLorry = "🚛";
		public const string KissingSmilingEyes = "😙";
		public const string Pear = "🍐";
		public const string Bicyclist = "🚴";
		public const string Rabbit2 = "🐇";
		public const string Clock830 = "🕣";
		public const string Train = "🚋";
		public const string OncomingAutomobile = "🚘";
		public const string Expressionless = "😑";
		public const string SmilingImp = "😈";
		public const string Frowning = "😦";
		public const string NoMouth = "😶";
		public const string BabyBottle = "🍼";
		public const string NonPotableWater = "🚱";
		public const string OpenMouth = "😮";
		public const string LastQuarterMoonWithFace = "🌜";
		public const string DoNotLitter = "🚯";
		public const string Sunglasses = "😎";
		public const string Loop = "➿";
		public const string LastQuarterMoon = "🌗";
		public const string Grinning = "😀";
		public const string Euro = "💶";
		public const string Clock330 = "🕞";
		public const string Telescope = "🔭";
		public const string GlobeWithMeridians = "🌐";
		public const string PostalHorn = "📯";
		public const string StuckOutTongue = "😛";
		public const string Clock1030 = "🕥";
		public const string Pound = "💷";
		public const string TwoMenHoldingHands = "👬";
		public const string Tiger2 = "🐅";
		public const string Anguished = "😧";
		public const string VerticalTrafficLight = "🚦";
		public const string Confused = "😕";
		public const string Repeat = "🔁";
		public const string OncomingPoliceCar = "🚔";
		public const string Tram = "🚊";
		public const string Dragon = "🐉";
		public const string EarthAmericas = "🌎";
		public const string RugbyFootball = "🏉";
		public const string LeftLuggage = "🛅";
		public const string Sound = "🔉";
		public const string Clock630 = "🕡";
		public const string DromedaryCamel = "🐪";
		public const string OncomingBus = "🚍";
		public const string HorseRacing = "🏇";
		public const string Rooster = "🐓";
		public const string Rowboat = "🚣";
		public const string Customs = "🛃";
		public const string RepeatOne = "🔂";
		public const string WaxingCrescentMoon = "🌒";
		public const string MountainRailway = "🚞";
		public const string Clock930 = "🕤";
		public const string PutLitterInItsPlace = "🚮";
		public const string ArrowsCounterclockwise = "🔄";
		public const string Clock130 = "🕜";
		public const string Goat = "🐐";
		public const string Pig2 = "🐖";
		public const string Innocent = "😇";
		public const string NoBicycles = "🚳";
		public const string LightRail = "🚈";
		public const string Whale2 = "🐋";
		public const string Train2 = "🚆";
		public const string EarthAfrica = "🌍";
		public const string Shower = "🚿";
		public const string WaningGibbousMoon = "🌖";
		public const string SteamLocomotive = "🚂";
		public const string Cat2 = "🐈";
		public const string Tractor = "🚜";
		public const string ThoughtBalloon = "💭";
		public const string TwoWomenHoldingHands = "👭";
		public const string FullMoonWithFace = "🌝";
		public const string Mouse2 = "🐁";
		public const string Clock430 = "🕟";
		public const string Worried = "😟";
		public const string Rat = "🐀";
		public const string Ram = "🐏";
		public const string Dog2 = "🐕";
		public const string Kissing = "😗";
		public const string Helicopter = "🚁";
		public const string Clock1130 = "🕦";
		public const string NoMobilePhones = "📵";
		public const string EuropeanPostOffice = "🏤";
		public const string Ox = "🐂";
		public const string MountainCableway = "🚠";
		public const string Sleeping = "😴";
		public const string Cow2 = "🐄";
		public const string Minibus = "🚐";
		public const string Clock730 = "🕢";
		public const string AerialTramway = "🚡";
		public const string Speaker = "🔈";
		public const string NoBell = "🔕";
		public const string MailboxWithMail = "📬";
		public const string NoPedestrians = "🚷";
		public const string Microscope = "🔬";
		public const string Bathtub = "🛁";
		public const string SuspensionRailway = "🚟";
		public const string Crocodile = "🐊";
		public const string MountainBicyclist = "🚵";
		public const string WaningCrescentMoon = "🌘";
		public const string Monorail = "🚝";
		public const string ChildrenCrossing = "🚸";
		public const string Clock230 = "🕝";
		public const string BustsInSilhouette = "👥";
		public const string MailboxWithNoMail = "📭";
		public const string Leopard = "🐆";
		public const string DeciduousTree = "🌳";
		public const string OncomingTaxi = "🚖";
		public const string Lemon = "🍋";
		public const string Mute = "🔇";
		public const string BaggageClaim = "🛄";
		public const string TwistedRightwardsArrows = "🔀";
		public const string SunWithFace = "🌞";
		public const string Trolleybus = "🚎";
		public const string EvergreenTree = "🌲";
		public const string PassportControl = "🛂";
		public const string NewMoonWithFace = "🌚";
		public const string PotableWater = "🚰";
		public const string HighBrightness = "🔆";
		public const string LowBrightness = "🔅";
		public const string Clock530 = "🕠";
		public const string Hushed = "😯";
		public const string Grimacing = "😬";
		public const string WaterBuffalo = "🐃";
		public const string NeutralFace = "😐";
		public const string Clock1230 = "🕧";
		public const string BodyBuilder = "🏋️";
		public const string FlexedArm = "💪";
		public const string RegionalIndicatorA = "🇦";
		public const string RegionalIndicatorB = "🇧";
		public const string RegionalIndicatorC = "🇨";
		public const string RegionalIndicatorD = "🇩";
		public const string RegionalIndicatorE = "🇪";
		public const string RegionalIndicatorF = "🇫";
		public const string RegionalIndicatorG = "🇬";
		public const string RegionalIndicatorH = "🇭";
		public const string RegionalIndicatorI = "🇮";
		public const string RegionalIndicatorJ = "🇯";
		public const string RegionalIndicatorK = "🇰";
		public const string RegionalIndicatorL = "🇱";
		public const string RegionalIndicatorM = "🇲";
		public const string RegionalIndicatorN = "🇳";
		public const string RegionalIndicatorO = "🇴";
		public const string RegionalIndicatorP = "🇵";
		public const string RegionalIndicatorQ = "🇶";
		public const string RegionalIndicatorR = "🇷";
		public const string RegionalIndicatorS = "🇸";
		public const string RegionalIndicatorT = "🇹";
		public const string RegionalIndicatorU = "🇺";
		public const string RegionalIndicatorV = "🇻";
		public const string RegionalIndicatorW = "🇼";
		public const string RegionalIndicatorX = "🇽";
		public const string RegionalIndicatorY = "🇾";
		public const string RegionalIndicatorZ = "🇿";

		public Dictionary<string, string> GetAllEmotes()
		{
			Dictionary<string, string> emotes = new Dictionary<string, string>();

			Emotes emoteClass = new Emotes();

			FieldInfo[] fields = emoteClass.GetType().GetFields();
			foreach (FieldInfo field in fields)
			{
				emotes.Add(field.Name, field.GetValue(emoteClass).ToString());
			}
			////foreach (System.Reflection.PropertyInfo prop in emoteClass.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			////{
			////}

			return emotes;
		}
	}
}