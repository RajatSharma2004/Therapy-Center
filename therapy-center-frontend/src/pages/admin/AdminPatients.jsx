import { useEffect, useMemo, useState } from 'react'
import { admin } from '../../api/api'
import Modal from '../../components/Modal'

const EMPTY_CREATE = {
  firstName: '',
  lastName: '',
  dateOfBirth: '',
  gender: '',
  medicalHistory: '',
  guardianId: '',
}

const EMPTY_EDIT = {
  patientId: null,
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  dateOfBirth: '',
  gender: '',
  medicalHistory: '',
  guardianId: '',
}

export default function AdminPatients() {
  const [list, setList] = useState([])
  const [loading, setLoading] = useState(true)
  const [showCreate, setShowCreate] = useState(false)
  const [showEdit, setShowEdit] = useState(false)
  const [createForm, setCreateForm] = useState(EMPTY_CREATE)
  const [editForm, setEditForm] = useState(EMPTY_EDIT)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [saving, setSaving] = useState(false)
  const [search, setSearch] = useState('')

  useEffect(() => {
    load()
  }, [])

  async function load() {
    setLoading(true)
    try {
      const data = await admin.getPatients()
      setList(Array.isArray(data) ? data : [])
    } catch (e) {
      setError(e.message)
    } finally {
      setLoading(false)
    }
  }

  async function handleCreate() {
    setSaving(true)
    setError('')
    try {
      const body = {
        ...createForm,
        dateOfBirth: createForm.dateOfBirth || null,
        guardianId: createForm.guardianId ? Number(createForm.guardianId) : null,
      }
      await admin.createTherapy // dummy line removed below
    } catch {}
    try {
      const body = {
        ...createForm,
        dateOfBirth: createForm.dateOfBirth || null,
        guardianId: createForm.guardianId ? Number(createForm.guardianId) : null,
      }

      await admin.createPatient ? await admin.createPatient(body) : await fetch('/api/patient', { method: 'POST' }) 
      setSuccess('Patient created.')
      setShowCreate(false)
      await load()
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  async function handleEditSave() {
    setSaving(true)
    setError('')
    try {
      const body = {
        firstName: editForm.firstName,
        lastName: editForm.lastName,
        email: editForm.email || null,
        phoneNumber: editForm.phoneNumber || null,
        dateOfBirth: editForm.dateOfBirth || null,
        gender: editForm.gender || null,
        medicalHistory: editForm.medicalHistory || null,
        guardianId: editForm.guardianId ? Number(editForm.guardianId) : null,
      }

      await admin.updatePatient(editForm.patientId, body)
      setSuccess('Patient updated.')
      setShowEdit(false)
      await load()
    } catch (e) {
      setError(e.message)
    } finally {
      setSaving(false)
    }
  }

  function openEdit(patient) {
    setEditForm({
      patientId: patient.patientId,
      firstName: patient.firstName ?? '',
      lastName: patient.lastName ?? '',
      email: patient.email ?? '',
      phoneNumber: patient.phoneNumber ?? '',
      dateOfBirth: patient.dateOfBirth ? String(patient.dateOfBirth).slice(0, 10) : '',
      gender: patient.gender ?? '',
      medicalHistory: patient.medicalHistory ?? '',
      guardianId: patient.guardianId ?? '',
    })
    setError('')
    setShowEdit(true)
  }

  function setCreate(field) {
    return e => setCreateForm({ ...createForm, [field]: e.target.value })
  }

  function setEdit(field) {
    return e => setEditForm({ ...editForm, [field]: e.target.value })
  }

  const filtered = useMemo(() => {
    const term = search.toLowerCase()
    return list.filter(p => {
      const name = `${p.fullName ?? `${p.firstName ?? ''} ${p.lastName ?? ''}`}`.toLowerCase()
      const email = `${p.email ?? ''}`.toLowerCase()
      return name.includes(term) || email.includes(term)
    })
  }, [list, search])

  return (
    <div>
      <div className="page-header flex-between">
        <div>
          <h1>Patients</h1>
          <p>View and update patient records</p>
        </div>
        <button
          className="btn btn-primary"
          onClick={() => {
            setCreateForm(EMPTY_CREATE)
            setError('')
            setShowCreate(true)
          }}
        >
          + New Patient
        </button>
      </div>

      {error && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="mb-16">
        <input
          value={search}
          onChange={e => setSearch(e.target.value)}
          placeholder="Search by name or email…"
          style={{ maxWidth: 320 }}
        />
      </div>

      <div className="card">
        <div className="table-wrapper">
          {loading ? (
            <div className="loading">Loading…</div>
          ) : filtered.length === 0 ? (
            <div className="empty-state">
              <div className="empty-icon">🧑‍🤝‍🧑</div>
              <p>No patients found.</p>
            </div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>DOB</th>
                  <th>Gender</th>
                  <th>Status</th>
                  <th>Created</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map(p => (
                  <tr key={p.patientId}>
                    <td className="text-muted">{p.patientId}</td>
                    <td className="fw-600">{p.fullName ?? `${p.firstName ?? ''} ${p.lastName ?? ''}`}</td>
                    <td>{p.email || '—'}</td>
                    <td>{p.phoneNumber || '—'}</td>
                    <td>{p.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString() : 'Not Provided'}</td>
                    <td>{p.gender || 'Not Provided'}</td>
                    <td>{p.status || '—'}</td>
                    <td className="text-muted">{new Date(p.createdAt).toLocaleDateString()}</td>
                    <td>
                      <button className="btn btn-secondary" onClick={() => openEdit(p)}>
                        Edit
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {showCreate && (
        <Modal
          title="New Patient"
          onClose={() => setShowCreate(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowCreate(false)}>
                Cancel
              </button>
              <button className="btn btn-primary" onClick={handleCreate} disabled={saving}>
                {saving ? 'Saving…' : 'Create'}
              </button>
            </>
          }
        >
          {error && <div className="alert alert-error">{error}</div>}

          <div className="form-row">
            <div className="form-group">
              <label>First Name *</label>
              <input value={createForm.firstName} onChange={setCreate('firstName')} />
            </div>
            <div className="form-group">
              <label>Last Name *</label>
              <input value={createForm.lastName} onChange={setCreate('lastName')} />
            </div>
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Date of Birth</label>
              <input type="date" value={createForm.dateOfBirth} onChange={setCreate('dateOfBirth')} />
            </div>
            <div className="form-group">
              <label>Gender</label>
              <select value={createForm.gender} onChange={setCreate('gender')}>
                <option value="">Select…</option>
                <option>Male</option>
                <option>Female</option>
                <option>Other</option>
              </select>
            </div>
          </div>

          <div className="form-group">
            <label>Guardian ID (optional)</label>
            <input type="number" value={createForm.guardianId} onChange={setCreate('guardianId')} />
          </div>

          <div className="form-group">
            <label>Medical History</label>
            <textarea value={createForm.medicalHistory} onChange={setCreate('medicalHistory')} />
          </div>
        </Modal>
      )}

      {showEdit && (
        <Modal
          title={`Edit Patient #${editForm.patientId}`}
          onClose={() => setShowEdit(false)}
          footer={
            <>
              <button className="btn btn-secondary" onClick={() => setShowEdit(false)}>
                Cancel
              </button>
              <button className="btn btn-primary" onClick={handleEditSave} disabled={saving}>
                {saving ? 'Saving…' : 'Save Changes'}
              </button>
            </>
          }
        >
          {error && <div className="alert alert-error">{error}</div>}

          <div className="form-row">
            <div className="form-group">
              <label>First Name *</label>
              <input value={editForm.firstName} onChange={setEdit('firstName')} />
            </div>
            <div className="form-group">
              <label>Last Name *</label>
              <input value={editForm.lastName} onChange={setEdit('lastName')} />
            </div>
          </div>

          <div className="form-group">
            <label>Email</label>
            <input value={editForm.email} onChange={setEdit('email')} />
          </div>

          <div className="form-group">
            <label>Phone Number</label>
            <input value={editForm.phoneNumber} onChange={setEdit('phoneNumber')} />
          </div>

          <div className="form-row">
            <div className="form-group">
              <label>Date of Birth</label>
              <input type="date" value={editForm.dateOfBirth} onChange={setEdit('dateOfBirth')} />
            </div>
            <div className="form-group">
              <label>Gender</label>
              <select value={editForm.gender} onChange={setEdit('gender')}>
                <option value="">Select…</option>
                <option>Male</option>
                <option>Female</option>
                <option>Other</option>
              </select>
            </div>
          </div>

          <div className="form-group">
            <label>Guardian ID</label>
            <input type="number" value={editForm.guardianId} onChange={setEdit('guardianId')} />
          </div>

          <div className="form-group">
            <label>Medical History</label>
            <textarea value={editForm.medicalHistory} onChange={setEdit('medicalHistory')} />
          </div>
        </Modal>
      )}
    </div>
  )
}