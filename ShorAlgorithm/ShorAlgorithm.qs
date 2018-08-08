namespace ShorAlgorithm
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;

	operation Set (desired: Result, q1: Qubit) : ()	// Set the state of q1, either |0> or |1>
    {
        body
        {
            let current = M(q1);
            if (desired != current)
            {
                X(q1);
            }
        }
    }

    operation Ux (x: Int, N: Int, y: Qubit[]) : ()	// Ux|y> = |xy % N>
	{
		body
		{
			let multiplier = LittleEndian(y);
			ModularMultiplyByConstantLE(x, N, multiplier);
		}
		controlled auto
	}

	operation QFT (q:Qubit[]) : ()	// Quantum Fourier Transform, whose adjoint is redefined
    {
        body
        {
			mutable n = Length(q);
			mutable pow = new Int [n+1];
			set pow[0] = 1;
			for (i in 1 .. n)
			{
				set pow[i] = 2*pow[i-1]; 
			}
            for (i in 0 .. n-1)
			{
				H(q[i]);
				for (j in i+1 .. n-1)
				{
					(controlled R1)([q[j]],(2*PI()/pow[j-i+1],q[i));
				}
			}
			for (i in 0 .. (n-1)/2)
			{
				SWAP(q[i],q[n-1-i]);
			}
        }
		adjoint
		{
			mutable n = Length(q);
			mutable pow = new Int [n+1];
			set pow[0] = 1;
			for (i in 1 .. n)
			{
				set pow[i] = 2 * pow[i-1]; 
			}
			for (i in 0 .. (n / 2 - 1))
			{
				SWAP(q[i], q[n-1-i]);
			}

			for (i in n-1 .. -1 .. 0)
			{
				for (j in n-1 .. -1 .. i+1)
				{
					(controlled R1)([q[j]], (-2*PI()/pow[j-i+1],q[i]));
				}
				H(q[i]);
			}
		}
	}
	operation PhaseEstimation(x: Int, N: Int, reg2: Qubit[], t: Int, L: Int) : (Int[])
	{
		body
		{
			mutable rtn = new Int[t];
			using (reg1 = Qubit[t]) 
			{
				for (i in 0 .. t-1)
				{
					Set(Zero, reg1[i]);
					H(reg1[i]);
				}

				mutable xx = x;
				for (i in 0 .. t-1) 
				{
					(Controlled Ux) ([reg1[t - i - 1]], (xx, N, reg2));	// use U_{x^j} instead of U_x^j

					set xx = xx * xx % N;
				}
				(Adjoint QFT) (reg1);	// Run Inverse Quantum Fourier Transform
			
				let abc = 1;

				for(i in 0 .. t - 1)
				{
					let result = M(reg1[i]);
					if(result == Zero)
					{
						set rtn[i] = 0;
					}
					if(result == One)
					{
						set rtn[i] = 1;
					}
				}
				ResetAll(reg1);
			}
			return rtn;
		}
	}
	operation QuantumOF (x: Int, N: Int, t: Int, L: Int) : (Double)	// the quantum part of Order Finding
	{
		body
		{
			mutable phi = 0.0;
			mutable two = 0.5;
			using (reg2 = Qubit[L])
			{
				for (i in 1 .. L - 1)
				{
					Set(Zero, reg2[i]);
				}
				Set(One, reg2[0]);

				let outcome = PhaseEstimation(x, N, reg2, t, L);

				for (i in 0 .. t - 1)
				{
					if (outcome[i] == 1)
					{
						set phi = phi + two;
					}
					set two = two * 0.5;
				}
				ResetAll(reg2);
			}
			return phi;
		}
	}
}
