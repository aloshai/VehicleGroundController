using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace VehicleGroundController
{
    public class VehicleGroundController : BaseScript
    {
        public Random random;
        public bool Developer = true;

        public MaterialHash surface;
        public VehicleGroundController()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
        }

        public void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            random = new Random();
            Tick += OnTick;
        }

        public async Task OnTick()
        {
            if (LocalPlayer.Character.CurrentVehicle != null)
            {
                if (LocalPlayer.Character.CurrentVehicle.IsStopped == false &&
                    !API.IsEntityInAir(LocalPlayer.Character.CurrentVehicle.Handle))
                {
                    var hash = GetGroundHash(LocalPlayer.Character.CurrentVehicle);

                    if (Developer == true)
                    {
                        if (surface != hash)
                        {
                            SendNotification("Surface: " + hash.ToString());
                        }
                        surface = hash;
                    }

                    if (hash != MaterialHash.Tarmac && hash != MaterialHash.Concrete)
                    {
                        API.SetVehicleSteerBias(LocalPlayer.Character.CurrentVehicle.Handle, Convert.ToSingle(-2 * random.NextDouble() + 1));
                        API.SetVehicleHandbrake(LocalPlayer.Character.CurrentVehicle.Handle, true);
                        await Delay(random.Next(10, 50));
                        API.SetVehicleHandbrake(LocalPlayer.Character.CurrentVehicle.Handle, false);
                    }
                }
            }

            await Delay(0);
        }

        public MaterialHash GetGroundHash(Vehicle vehicle)
        {
            var vector = API.GetEntityCoords(vehicle.Handle, true);
            var num = API.StartShapeTestCapsule(vector.X, vector.Y, vector.Z + 4f, vector.X, vector.Y, vector.Z - 2.0f, 1f,
                1, vehicle.Handle, 7);

            bool hit = false;
            Vector3 endCoords = Vector3.Zero, surfaceNormal = Vector3.Zero;
            uint materialHash = 0;
            int entityHit = 0;

            var result = API.GetShapeTestResultEx(num, ref hit, ref endCoords, ref surfaceNormal, ref materialHash, ref entityHit);

            if (result == 0)
            {
                return MaterialHash.None;
            }

            return (MaterialHash)materialHash;
        }

        public void SendNotification(string text)
        {
            API.SetNotificationTextEntry("STRING");
            API.AddTextComponentString(text);
            API.DrawNotification(false, false);
        }
    }
}
