/*
 * POWER
 */
PowerDetails powerDetails;

public class PowerDetails {
    public Program program;
    public Template template;
    public List<IMyPowerProducer> powerProducerBlocks;
    public List<IMyJumpDrive> jumpDriveBlocks;
    public List<MyInventoryItem> items;

    public int jumpDrives;
    public float jumpMax;
    public float jumpCurrent;

    public int batteries;
    public float batteryMax;
    public float batteryCurrent;

    public int reactors;
    public float reactorOutputMW;
    public MyFixedPoint reactorUranium;

    public int solars;
    public float solarOutputMW;
    public float solarOutputMax;

    public PowerDetails(Program program, Template template = null) {
        this.program = program;
        this.template = template;
        this.powerProducerBlocks = new List<IMyPowerProducer>();
        this.jumpDriveBlocks = new List<IMyJumpDrive>();
        this.items = new List<MyInventoryItem>();
        this.jumpDrives = 0;
        this.jumpMax = 0f;
        this.jumpCurrent = 0f;
        this.batteries = 0;
        this.batteryMax = 0f;
        this.batteryCurrent = 0f;
        this.reactors = 0;
        this.reactorOutputMW = 0f;
        this.reactorUranium = 0;
        this.solars = 0;
        this.solarOutputMW = 0f;
        this.solarOutputMax = 0f;
        this.GetBlocks();
        this.RegisterTemplateVars();
    }

    public void RegisterTemplateVars() {
        if (this.template == null) {
            return;
        }
        this.template.Register("power.jumpDrives", () => this.jumpDrives.ToString());
        this.template.Register("power.jumpBar",
            (DrawingSurface ds, string text, Dictionary<string, string> options) => {
                float pct = this.GetPercent(this.jumpCurrent, this.jumpMax);
                ds.Bar(pct, text: Util.PctString(pct));
            });
        this.template.Register("power.batteries", () => this.batteries.ToString());
        this.template.Register("power.batteryBar",
            (DrawingSurface ds, string text, Dictionary<string, string> options) => {
                float pct = this.GetPercent(this.batteryCurrent, this.batteryMax);
                ds.Bar(pct, text: Util.PctString(pct));
            });
        this.template.Register("power.solars", () => this.solars.ToString());
        this.template.Register("power.reactors", () => this.reactors.ToString());
        this.template.Register("power.reactorMw", () => this.reactorOutputMW.ToString());
        this.template.Register("power.reactorUr", () => $"{Util.FormatNumber(reactorUranium)} kg");
    }

    public void Clear() {
        this.jumpDrives = 0;
        this.jumpMax = 0f;
        this.jumpCurrent = 0f;
        this.batteries = 0;
        this.batteryMax = 0f;
        this.batteryCurrent = 0f;
        this.reactors = 0;
        this.reactorOutputMW = 0f;
        this.reactorUranium = 0;
        this.solars = 0;
        this.solarOutputMW = 0f;
        this.solarOutputMax = 0f;
    }

    public void GetBlocks() {
        this.powerProducerBlocks.Clear();
        this.program.GridTerminalSystem.GetBlocksOfType<IMyPowerProducer>(this.powerProducerBlocks, b => b.IsSameConstructAs(this.program.Me));
        this.jumpDriveBlocks.Clear();
        this.program.GridTerminalSystem.GetBlocksOfType<IMyJumpDrive>(this.jumpDriveBlocks, b => b.IsSameConstructAs(this.program.Me));
    }

    public float GetPercent(float current, float max) {
        if (max == 0) {
            return 0f;
        }
        return current / max;
    }

    public void Refresh() {
        Clear();

        foreach (IMyPowerProducer powerBlock in this.powerProducerBlocks) {
            if (powerBlock is IMyBatteryBlock) {
                this.batteries += 1;
                this.batteryCurrent += ((IMyBatteryBlock)powerBlock).CurrentStoredPower;
                this.batteryMax += ((IMyBatteryBlock)powerBlock).MaxStoredPower;
            } else if (powerBlock is IMyReactor) {
                this.reactors += 1;
                this.reactorOutputMW += ((IMyReactor)powerBlock).CurrentOutput;

                this.items.Clear();
                var inv = ((IMyReactor)powerBlock).GetInventory(0);
                inv.GetItems(this.items);
                for (var i = 0; i < items.Count; i++) {
                    this.reactorUranium += items[i].Amount;
                }
            } else if (powerBlock is IMySolarPanel) {
                this.solars += 1;
                this.solarOutputMW += ((IMySolarPanel)powerBlock).CurrentOutput;
                this.solarOutputMax += ((IMySolarPanel)powerBlock).MaxOutput;
            }
        }

        foreach (IMyJumpDrive jumpDrive in jumpDriveBlocks) {
            this.jumpDrives += 1;
            this.jumpCurrent += jumpDrive.CurrentStoredPower;
            this.jumpMax += jumpDrive.MaxStoredPower;
        }
    }

    public override string ToString() {
        return
            $"{this.jumpDrives} Jump drive{Util.Plural(this.jumpDrives, "", "s")}:\n" +
            $"{this.jumpCurrent} / {this.jumpMax}\n" +
            $"{this.batteries} Batter{Util.Plural(this.batteries, "y", "ies")}\n" +
            $"{this.batteryCurrent} / {this.batteryMax}\n" +
            $"{this.reactors} Reactor{Util.Plural(this.reactors, "", "s")}\n" +
            $"{this.reactorOutputMW} MW, {Util.FormatNumber(this.reactorUranium)} Fuel";
    }
}
/* POWER */
