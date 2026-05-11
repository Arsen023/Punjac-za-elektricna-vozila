using System;
using System.ServiceModel;

namespace EVCharger.Common
{
    public static class ChargingServiceValidator
    {
        public static void ThrowIfInvalidVehicleId(string vehicleId)
        {
            if (string.IsNullOrWhiteSpace(vehicleId))
            {
                ThrowFault("VehicleId je obavezan i ne sme biti prazan.", vehicleId, -1);
            }
        }

        public static void ThrowIfInvalidSample(ChargingSample sample)
        {
            if (sample == null)
            {
                ThrowFault("Uzorak (sample) ne sme biti null.", null, -1);
            }

            if (string.IsNullOrWhiteSpace(sample.VehicleId))
            {
                ThrowFault("VehicleId u uzorku je obavezan.", sample.VehicleId, sample.RowIndex);
            }

            if (sample.Timestamp == default(DateTime))
            {
                ThrowFault("Timestamp mora biti postavljen (ne sme biti podrazumevani DateTime).", sample.VehicleId, sample.RowIndex);
            }

            ThrowIfInvalidAggregate(sample.VoltageRms, "VoltageRms", sample, requireStrictlyPositive: true, nonNegativeOnly: false);
            ThrowIfInvalidAggregate(sample.Frequency, "Frequency", sample, requireStrictlyPositive: true, nonNegativeOnly: false);
            ThrowIfInvalidAggregate(sample.CurrentRms, "CurrentRms", sample, requireStrictlyPositive: false, nonNegativeOnly: true);
            ThrowIfInvalidAggregate(sample.RealPower, "RealPower", sample, requireStrictlyPositive: false, nonNegativeOnly: true);
            ThrowIfInvalidAggregateFiniteOnly(sample.ReactivePower, "ReactivePower", sample);
            ThrowIfInvalidAggregate(sample.ApparentPower, "ApparentPower", sample, requireStrictlyPositive: false, nonNegativeOnly: true);
        }

        private static void ThrowIfInvalidAggregateFiniteOnly(RmsAggregate aggregate, string name, ChargingSample sample)
        {
            if (aggregate == null)
            {
                ThrowFault($"Polje {name} je obavezno (null nije dozvoljen).", sample.VehicleId, sample.RowIndex);
            }

            if (!IsFinite(aggregate.Min) || !IsFinite(aggregate.Avg) || !IsFinite(aggregate.Max))
            {
                ThrowFault($"{name}: Min, Avg i Max moraju biti konačni brojevi (bez NaN/Infinity).", sample.VehicleId, sample.RowIndex);
            }
        }

        private static void ThrowIfInvalidAggregate(
            RmsAggregate aggregate,
            string name,
            ChargingSample sample,
            bool requireStrictlyPositive,
            bool nonNegativeOnly = false)
        {
            if (aggregate == null)
            {
                ThrowFault($"Polje {name} je obavezno (null nije dozvoljen).", sample.VehicleId, sample.RowIndex);
            }

            if (!IsFinite(aggregate.Min) || !IsFinite(aggregate.Avg) || !IsFinite(aggregate.Max))
            {
                ThrowFault($"{name}: Min, Avg i Max moraju biti konačni brojevi (bez NaN/Infinity).", sample.VehicleId, sample.RowIndex);
            }

            if (requireStrictlyPositive)
            {
                if (aggregate.Min <= 0d || aggregate.Avg <= 0d || aggregate.Max <= 0d)
                {
                    ThrowFault($"{name}: Min, Avg i Max moraju biti veći od 0 (zahtev specifikacije za napon/frekvenciju).", sample.VehicleId, sample.RowIndex);
                }
            }
            else if (nonNegativeOnly)
            {
                if (aggregate.Min < 0d || aggregate.Avg < 0d || aggregate.Max < 0d)
                {
                    ThrowFault($"{name}: Min, Avg i Max ne smeju biti negativni.", sample.VehicleId, sample.RowIndex);
                }
            }
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        private static void ThrowFault(string message, string vehicleId, int rowIndex)
        {
            var detail = new ChargingValidationFault
            {
                Message = message,
                VehicleId = vehicleId,
                RowIndex = rowIndex
            };

            throw new FaultException<ChargingValidationFault>(
                detail,
                new FaultReason(message),
                new FaultCode("Validation"));
        }
    }
}
