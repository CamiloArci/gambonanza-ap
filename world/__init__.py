from .Items import item_table
from .Locations import location_table
from world.AutoWorld import World, WebWorld
from BaseClasses import Region, Location, Item, ItemClassification, Entrance, Tutorial
from typing import List

class GambonanzaWebWorld(WebWorld):
    theme = "ice" # Example theme
    tutorials = [Tutorial(
        "Gambonanza Archipelago Setup Guide",
        "A guide to setting up the Gambonanza randomizer mod.",
        "English",
        "setup_en.md",
        "setup/en",
        ["Arci"]
    )]

class GambonanzaWorld(World):
    """
    Gambonanza is a chess-based roguelike where you build a team of pieces and gambits.
    """
    game = "Gambonanza"
    web = GambonanzaWebWorld()

    item_name_to_id = {name: data.code for name, data in item_table.items()}
    location_name_to_id = {name: data.code for name, data in location_table.items()}

    def create_items(self):
        item_pool: List[Item] = []
        for name, data in item_table.items():
            count = 7 if name == "Board Upgrade" else 1
            for _ in range(count):
                item_pool.append(self.create_item(name))
        self.multiworld.itempool += item_pool

    def create_regions(self):
        # Create Main Region
        menu_region = Region("Menu", self.player, self.multiworld)
        self.multiworld.regions.append(menu_region)

        # In a roguelike, we can just put all locations in one big 'Game' region 
        # since there are no item-based locks.
        game_region = Region("Game", self.player, self.multiworld)
        for name, data in location_table.items():
            game_region.locations.append(GambonanzaLocation(self.player, name, data.code, game_region))
        self.multiworld.regions.append(game_region)

        # Connect Menu -> Game
        menu_region.connect(game_region)

    def fill_slot_data(self) -> dict:
        return {
            "seed": self.multiworld.per_slot_randoms[self.player].randint(0, 2147483647)
        }

    def create_item(self, name: str) -> Item:
        data = item_table[name]
        return Item(name, data.classification, data.code, self.player)

class GambonanzaLocation(Location):
    game = "Gambonanza"
